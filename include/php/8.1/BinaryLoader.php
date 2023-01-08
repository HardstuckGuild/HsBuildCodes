<?php namespace Hardstuck\GuildWars2\BuildCodes\V2;

use Hardstuck\GuildWars2\BuildCodes\V2\Util\AllEquipmentInfusions;
use Hardstuck\GuildWars2\BuildCodes\V2\Util\AllEquipmentStats;
use Hardstuck\GuildWars2\BuildCodes\V2\Util\StringView;
use Hardstuck\GuildWars2\BuildCodes\V2\Util\TraitLineChoices;

class BitReader {
	public string $Data;
	public int    $BitPos;

	public function __construct(string $data) {
		$this->Data   = $data;
		$this->BitPos = 0;
	}

	//NOTE(Rennorb): its more efficient to not use this, but its really handy
	public function EatIfExpected(int $expected, int $width) : bool
	{
		$val = $this->DecodeNext($width);
		if($val !== $expected) $this->BitPos -= $width;
		return $val === $expected;
	}

	public function DecodeNext_GetMinusMinIfAtLeast(int $min, int $width) : int
	{
		$val = 0;
		$this->DecodeNext_WriteMinusMinIfAtLeast($val, $min, $width);
		return $val;
	}
	public function DecodeNext_WriteMinusMinIfAtLeast(mixed &$target, int $min, int $width) : void
	{
		$id = $this->DecodeNext($width);
		$actualValue = $id - $min;
		if($id >= $min) $target = $actualValue;
	}

	public function DecodeNext(int $width) : int
	{
		//NOTE(Rennorb): this is way overcompluicated
		//TODO(Rennorb): @cleanup refactor this mess

		$sourceByteStart = $this->BitPos >> 3;
		$sourceByteWidth = (($this->BitPos & 7) + $width + 7) >> 3;
		//sadly required because  BinaryPrimitives.ReadInt32BigEndian always wants to decode 4 bytes
		$containsData = [0, 0, 0, 0];
		$additionalBytes = count($containsData) - $sourceByteWidth;
		for($i = 0; $i < $sourceByteWidth; $i++)
			$containsData[$additionalBytes + $i] = ord($this->Data[$sourceByteStart + $i]);
		$bitShiftRight = 8 - ($this->BitPos + $width) & 7;
		$bitShiftLeft = (($this->BitPos & 7) - (8 - ($width & 7))) & 7;
		for($i = 3; $i > $additionalBytes; $i--)
		{
			$containsData[$i] >>= $bitShiftRight;
			$containsData[$i] |= ($containsData[$i - 1] << $bitShiftLeft) & 0xFF;
		}
		$firstTargetByte = count($containsData) - (($width + 7) >> 3);
		if($firstTargetByte !== $additionalBytes) $containsData[$additionalBytes] = 0;
		else $containsData[$additionalBytes] >>= $bitShiftRight;
		$containsData[$firstTargetByte] &= ((1 << $width) - 1) & 0xFF;

		$this->BitPos += $width;

		return current(unpack('N', pack("CCCC", ...$containsData)));
	}

	public function DebugPrint() : string
	{
		$s = "";
		for($i = 0; $i < ord($this->Data); $i++)
		{
			if($i > 0 && $i % 8 === 0) $s += "| ";

			if($i === $this->BitPos / 8)
				$s += "_[".str_pad(decbin(ord($this->Data[$i])), 8, '0', STR_PAD_LEFT).']';
			else $s += '_'.str_pad(decbin(ord($this->Data[$i])), 8, '0', STR_PAD_LEFT);
		}
		return $s;
	}
}

class BitWriter {
	/** @var int[] */
	public array $Data   = [];
	public int   $BitPos = 0;

	public function Write(int $value, int $bitWidth) : void
	{
		$posInByte = $this->BitPos & 7;
		$bytesTouched = ($posInByte + $bitWidth + 7) >> 3;

		$buffer = pack('N', $value << (32 - $bitWidth - $posInByte));

		$dest = $this->BitPos >> 3;
		for($i = 0; $i < $bytesTouched; $i++)
			$this->Data[$dest + $i] = (array_key_exists($dest + $i, $this->Data) ? $this->Data[$dest + $i] : 0) | ord($buffer[$i]);

		$this->BitPos += $bitWidth;
	}
	
	public function DebugPrint() : string
	{
		$s = "";
		for($i = 0; $i < count($this->Data); $i++)
		{
			if($i > 0 && $i % 8 === 0) $s += "| ";

			if($i === $this->BitPos / 8)
				$s += "_[".str_pad(decbin($this->Data[$i]), 8, '0', STR_PAD_LEFT).']';
			else $s += '_'.str_pad(decbin($this->Data[$i]), 8, '0', STR_PAD_LEFT);
		}
		return $s;
	}
}

class BinaryLoader {
	use Util\_Static;

	#region hardstuck codes

	public static function LoadBuildCode(string $raw) : BuildCode
	{
		$rawSpan = new BitReader($raw);

		$code = new BuildCode();
		$code->Version = $rawSpan->DecodeNext(8) - ord('a');
		if($code->Version < FIRST_VERSIONED_VERSION || $code->Version > CURRENT_VERSION) throw new \Exception("Code version mismatch");
		$code->Kind    = match ($rawSpan->DecodeNext(2)) {
			0 => Kind::PvP,
			1 => Kind::WvW,
			2 => Kind::PvE,
			default => Kind::_UNDEFINED,
		};
		assert($code->Kind !== Kind::_UNDEFINED, "Code type not valid");
		$code->Profession = Profession::from(1 + $rawSpan->DecodeNext(4));

		for($i = 0; $i < 3; $i++) {
			$traitLine = $rawSpan->DecodeNext(7);
			if($traitLine !== SpecializationId::_UNDEFINED) {
				$choices = new TraitLineChoices();
				for($j = 0; $j < 3; $j++)
					$choices[$j] = TraitLineChoice::from($rawSpan->DecodeNext(2));
				$code->Specializations[$i] = new Specialization($traitLine, $choices);
			}
		}
		if(!$rawSpan->EatIfExpected(0, 5)) {
			$code->WeaponSet1 = BinaryLoader::LoadWeaponSet($rawSpan);
			if(!$rawSpan->EatIfExpected(0, 5))
				$code->WeaponSet2 = BinaryLoader::LoadWeaponSet($rawSpan);
		}
		for($i = 0; $i < 5; $i++)
			$code->SlotSkills[$i] = $rawSpan->DecodeNext(24);

		$code->Rune = $rawSpan->DecodeNext(24);
		
		if($code->Kind !== Kind::PvP)
			$code->EquipmentAttributes = BinaryLoader::LoadAllEquipmentStats($rawSpan, $code);
		else
			$code->EquipmentAttributes->Amulet = $rawSpan->DecodeNext(16);

		if($code->Kind !== Kind::PvP) {
			if(!$rawSpan->EatIfExpected(0, 24))
				$code->Infusions = BinaryLoader::LoadAllEquipmentInfusions($rawSpan, $code);

			$code->Food    = $rawSpan->DecodeNext(24);
			$code->Utility = $rawSpan->DecodeNext(24);
		}
		$code->ProfessionSpecific = BinaryLoader::LoadProfessionSpecific($rawSpan, $code->Profession);
		$code->Arbitrary          = BinaryLoader::LoadArbitrary($rawSpan);

		return $code;
	}

	private static function LoadWeaponSet(BitReader $rawSpan) : WeaponSet
	{
		$set = new WeaponSet();
		$set->MainHand = WeaponType::from(WeaponType::_FIRST() + $rawSpan->DecodeNext_GetMinusMinIfAtLeast(2, 5));
		$set->Sigil1 = $rawSpan->DecodeNext(24);
		if($set->MainHand !== WeaponType::_UNDEFINED && !IsTwoHanded($set->MainHand))
			$set->OffHand = WeaponType::from(WeaponType::_FIRST() + $rawSpan->DecodeNext_GetMinusMinIfAtLeast(2, 5));
		$set->Sigil2 = $rawSpan->DecodeNext(24);
		return $set;
	}

	private static function LoadAllEquipmentStats(BitReader $rawSpan, BuildCode $weaponRef) : AllEquipmentStats
	{
		$allData = new AllEquipmentStats();

		$repeatCount = 0;
		$data = StatId::_UNDEFINED;
		for($i = 0; $i < ALL_EQUIPMENT_COUNT; $i++) {
			if($repeatCount === 0) {
				$data = $rawSpan->DecodeNext(16);

				if($i === ALL_EQUIPMENT_COUNT - 1) $repeatCount = 1;
				else $repeatCount = $rawSpan->DecodeNext(4) + 1;
			}

			if(!HasAttributeSlot($weaponRef, $i)) continue;

			$allData[$i] = $data;
			$repeatCount--;
		}
		return $allData;
	}

	private static function LoadAllEquipmentInfusions(BitReader $rawSpan, BuildCode $weaponRef) : AllEquipmentInfusions
	{
		$allData = new AllEquipmentInfusions();

		$repeatCount = 0;
		$data = ItemId::_UNDEFINED;
		for($i = 0; $i < ALL_INFUSION_COUNT; $i++) {
			if($repeatCount === 0) {
				$data = $rawSpan->DecodeNext_GetMinusMinIfAtLeast(1, 24);

				if($i === ALL_EQUIPMENT_COUNT - 1) $repeatCount = 1;
				else $repeatCount = $rawSpan->DecodeNext(5) + 1;
			}

			if(!HasInfusionSlot($weaponRef, $i)) continue;

			$allData[$i] = $data;
			$repeatCount--;
		}
		return $allData;
	}

	private static function LoadProfessionSpecific(BitReader $rawSpan, Profession $profession) : IProfessionSpecific
	{
		switch($profession)
		{
			case Profession::Ranger: {
				$data = new RangerData();
				if(!$rawSpan->EatIfExpected(0, 7)) {
					$rawSpan->DecodeNext_WriteMinusMinIfAtLeast($data->Pet1, 2, 7);
					$rawSpan->DecodeNext_WriteMinusMinIfAtLeast($data->Pet2, 2, 7);
				}
				return $data;
			}

			case Profession::Revenant: {
				$data = new RevenantData();
				$data->Legend1 = $rawSpan->DecodeNext(4);
				if(!$rawSpan->EatIfExpected(0, 4)){
					$data->Legend2 = $rawSpan->DecodeNext(4);
					$rawSpan->DecodeNext_WriteMinusMinIfAtLeast($data->AltUtilitySkill1, 1, 24);
					$rawSpan->DecodeNext_WriteMinusMinIfAtLeast($data->AltUtilitySkill2, 1, 24);
					$rawSpan->DecodeNext_WriteMinusMinIfAtLeast($data->AltUtilitySkill3, 1, 24);
				}
				return $data;
			}

			default: return ProfessionSpecific\NONE::GetInstance();
		}
	}

	private static function LoadArbitrary(BitReader $rawSpan) : IArbitrary
	{
		//implement extensions here in the future
		return Arbitrary\NONE::GetInstance();
	}

	public static function WriteBuildCode(BuildCode $code) : string
	{
		$rawBits = new BitWriter();
		$rawBits->Data[0] = ord('a') + $code->Version;
		$rawBits->BitPos += 8;
		
		$rawBits->Write(match ($code->Kind) {
			Kind::PvP => 0,
			Kind::WvW => 1,
			Kind::PvE => 2,
			default => throw new \InvalidArgumentException("code->Kind"),
		}, 2);
		
		$rawBits->Write($code->Profession->value - 1, 4);

		for($i = 0; $i < 3; $i++)
		{
			if($code->Specializations[$i]->SpecializationId === SpecializationId::_UNDEFINED) $rawBits->Write(0, 7);
			else
			{
				$rawBits->Write($code->Specializations[$i]->SpecializationId, 7);
				for($j = 0; $j < 3; $j++)
					$rawBits->Write($code->Specializations[$i]->Choices[$j]->value, 2);
			}
		}

		if(!$code->WeaponSet1->HasAny() && !$code->WeaponSet2->HasAny()) $rawBits->Write(0, 5);
		else
		{
			$rawBits->Write(1 + $code->WeaponSet1->MainHand->value, 5);
			$rawBits->Write($code->WeaponSet1->Sigil1, 24);
			$rawBits->Write(1 + $code->WeaponSet1->OffHand->value, 5);
			$rawBits->Write($code->WeaponSet1->Sigil2, 24);

			if(!$code->WeaponSet2->HasAny()) $rawBits->Write(0, 5);
			else
			{
				$rawBits->Write(1 + $code->WeaponSet2->MainHand->value, 5);
				$rawBits->Write($code->WeaponSet2->Sigil1, 24);
				$rawBits->Write(1 + $code->WeaponSet2->OffHand->value, 5);
				$rawBits->Write($code->WeaponSet2->Sigil2, 24);
			}
		}

		for($i = 0; $i < 5; $i++)
			$rawBits->Write($code->SlotSkills[$i], 24);

		$rawBits->Write($code->Rune, 24);

		if($code->Kind === Kind::PvP) $rawBits->Write($code->EquipmentAttributes->Amulet, 16);
		else
		{
			{
				$lastStat = null;
				$repeatCount = 0;
				for($i = 0; $i < ALL_EQUIPMENT_COUNT; $i++)
				{
					if(!HasAttributeSlot($code, $i)) continue;

					if($code->EquipmentAttributes[$i] !== $lastStat)
					{
						if($lastStat !== null)
						{
							$rawBits->Write($lastStat, 16);
							$rawBits->Write($repeatCount - 1, 4);
						}

						$lastStat = $code->EquipmentAttributes[$i];
						$repeatCount = 1;
					}
					else
					{
						$repeatCount++;
					}
				}

				$rawBits->Write($lastStat, 16);
				if($repeatCount > 1)
					$rawBits->Write($repeatCount - 1, 4);
			}

			if(!$code->Infusions->HasAny()) $rawBits->Write(0, 24);
			else
			{
				$lastInfusion = ItemId::_UNDEFINED;
				$repeatCount = 0;
				for($i = 0; $i < ALL_INFUSION_COUNT; $i++)
				{
					if(!HasInfusionSlot($code, $i)) continue;

					if($code->Infusions[$i] !== $lastInfusion)
					{
						if($lastInfusion !== ItemId::_UNDEFINED)
						{
							$rawBits->Write($lastInfusion + 1, 24);
							$rawBits->Write($repeatCount - 1, 5);
						}

						$lastInfusion = $code->Infusions[$i];
						$repeatCount = 1;
					}
					else
					{
						$repeatCount++;
					}
				}

				$rawBits->Write($lastInfusion + 1, 24);
				if($repeatCount > 1)
					$rawBits->Write($repeatCount - 1, 5);
			}

			$rawBits->Write($code->Food, 24);
			$rawBits->Write($code->Utility, 24);
		}

		switch($code->Profession)
		{
			case Profession::Ranger:
				/** @var RangerData */
				$rangerData = $code->ProfessionSpecific;
				if($rangerData->Pet1 === PetId::_UNDEFINED && $rangerData->Pet2 === PetId::_UNDEFINED) $rawBits->Write(0, 7);
				else
				{
					$rawBits->Write(1 + $rangerData->Pet1, 7);
					$rawBits->Write(1 + $rangerData->Pet2, 7);
				}
				break;

			case Profession::Revenant:
				/** @var RevenantData */
				$revenantData = $code->ProfessionSpecific;
				$rawBits->Write($revenantData->Legend1, 4);
				if($revenantData->Legend2 === Legend::_UNDEFINED) $rawBits->Write(0, 4);
				else
				{
					$rawBits->Write($revenantData->Legend2, 4);
					$rawBits->Write($revenantData->AltUtilitySkill1, 24);
					$rawBits->Write($revenantData->AltUtilitySkill2, 24);
					$rawBits->Write($revenantData->AltUtilitySkill3, 24);
				}
				break;
		}

		return pack('C*', ...$rawBits->Data);
	}

	#endregion

	#region official codes

	/**
	 * @param string $raw binary string
	 * @remarks Requires PerProfessionData to be loaded or PerProfessionData::$LazyLoadMode to be set to something other than LazyLoadMode::NONE.
	 */
	public static function LoadOfficialBuildCode(string $raw, bool $aquatic = false) : BuildCode
	{
		$rawView = new StringView($raw);
		$codeType = $rawView->NextByte();
		assert($codeType === 0x0D);

		$code = new BuildCode();
		$code->Version    = CURRENT_VERSION;
		$code->Kind       = Kind::PvE;
		$code->Profession = Profession::from($rawView->NextByte());

		if(PerProfessionData::$LazyLoadMode >= LazyLoadMode::OFFLINE_ONLY) PerProfessionData::Reload($code->Profession, PerProfessionData::$LazyLoadMode < LazyLoadMode::FULL);
		$professionData = PerProfessionData::ByProfession($code->Profession);

		for($i = 0; $i < 3; $i++) {
			$spec = $rawView->NextByte();
			$mix = $rawView->NextByte();
			if($spec === SpecializationId::_UNDEFINED) continue;

			$choices = new TraitLineChoices();
			for($j = 0; $j < 3; $j++) {
				$choices[$j] = TraitLineChoice::from(($mix >> ($j * 2)) & 0b00000011);
			}
			$code->Specializations[$i] = new Specialization($spec, $choices);
		}

		$offset = $aquatic ? 2 : 0;
		$specRaw = $rawView->Slice(5 * 4);
		$rawView->Pos += $offset;

		switch($code->Profession)
		{
			case Profession::Ranger:
				$specRaw->Pos += $offset;

				$rangerData = new RangerData();
				if($specRaw[0] !== 0) $rangerData->Pet1 = $specRaw[0];
				if($specRaw[1] !== 0) $rangerData->Pet2 = $specRaw[1];

				$code->ProfessionSpecific = $rangerData;
				break;

			case Profession::Revenant:
				$specRaw->Pos += $offset;

				$revenantData = new RevenantData();
				
				if($specRaw[0] !== 0)
				{
					$revenantData->Legend1 = $specRaw[0] - Legend::_FIRST();

					for($i = 0; $i < 5; $i++) {
						$palletteId = $rawView->NextUShortLE();
						$rawView->Pos += 2;
						$code->SlotSkills[$i] = Overrides::RevPalletteToSkill($revenantData->Legend1, $palletteId);
					}
				}
				else
				{
					//NOTE(Rennorb): no legend available, here we can only guess the right skils.
					BinaryLoader::ReadSlotSkillsNormally($code, $professionData, $rawView);
				}

				if($specRaw[1] !== 0)
				{
					$revenantData->Legend2 = $specRaw[1] - Legend::_FIRST();
					$revSkillOffset = $aquatic ? 6: 2;
					$specRaw->Pos += $revSkillOffset;

					$rawSkills = array_values(unpack("v3", $specRaw->Data, $specRaw->Pos));

					$altSkills = [
						Overrides::RevPalletteToSkill($revenantData->Legend2, $rawSkills[0]),
						Overrides::RevPalletteToSkill($revenantData->Legend2, $rawSkills[1]),
						Overrides::RevPalletteToSkill($revenantData->Legend2, $rawSkills[2]),
					];

					if($specRaw[0] !== 0)
					{
						if($altSkills[0] !== 0) $revenantData->AltUtilitySkill1 = $altSkills[0];
						if($altSkills[1] !== 0) $revenantData->AltUtilitySkill2 = $altSkills[1];
						if($altSkills[2] !== 0) $revenantData->AltUtilitySkill3 = $altSkills[2];
					}
					else //flip skills so the first legend is always set
					{
						$revenantData->Legend1 = $revenantData->Legend2;
						$revenantData->Legend2 = Legend::_UNDEFINED;

						$revenantData->AltUtilitySkill1 = $code->SlotSkills->Utility1;
						$revenantData->AltUtilitySkill2 = $code->SlotSkills->Utility2;
						$revenantData->AltUtilitySkill3 = $code->SlotSkills->Utility3;

						for($i = 0; $i < 3; $i++)
							if($altSkills[$i] !== 0)
								$code->SlotSkills[1 + $i] = $altSkills[$i];
					}
				}

				$code->ProfessionSpecific = $revenantData;
				return $code;
		}

		BinaryLoader::ReadSlotSkillsNormally($code, $professionData, $rawView);

		return $code;
	}

	private static function ReadSlotSkillsNormally(BuildCode $code, PerProfessionData $skillData, StringView $raw) : void
	{
		for($i = 0; $i < 5; $i++) {
			$palletteId = $raw->NextUShortLE();
			if($palletteId !== 0) $code->SlotSkills[$i] = $skillData->PalletteToSkill[$palletteId];
			$raw->Pos += 2;
		}
	}

	/** @remarks Requires PerProfessionData to be loaded or PerProfessionData::$LazyLoadMode to be set to something other than LazyLoadMode::NONE. */
	public static function WriteOfficialBuildCode(BuildCode $code, bool $aquatic = false) : string
	{
		$destination = '';
		if(PerProfessionData::$LazyLoadMode >= LazyLoadMode::OFFLINE_ONLY) PerProfessionData::Reload($code->Profession, PerProfessionData::$LazyLoadMode < LazyLoadMode::FULL);
		$professionData = PerProfessionData::ByProfession($code->Profession);

		$destination .= chr(0x0d); //code type
		$destination .= chr($code->Profession->value);
		for($i = 0; $i < 3; $i++) {
			if($code->Specializations[$i]->SpecializationId === SpecializationId::_UNDEFINED) continue;

			$spec = $code->Specializations[$i];
			$destination .= chr($spec->SpecializationId);
			$destination .= chr($spec->Choices[0]->value | ($spec->Choices[1]->value << 2) | ($spec->Choices[2]->value << 4));
		}

		if($aquatic) $destination .= pack('x2');
		for($i = 0; $i < 5; $i++) {
			$palletteIndex = $professionData->SkillToPallette[$code->SlotSkills[$i]];
			$destination .= pack('vx2', $palletteIndex);
		}
		if(!$aquatic) $destination .= pack('x2');

		switch($code->Profession)
		{
			case Profession::Ranger:
				/** @var RangerData */
				$rangerData = $code->ProfessionSpecific;
				if($aquatic) $destination .= pack('x2');
				$destination .= chr(1 + $rangerData->Pet1);
				$destination .= chr(1 + $rangerData->Pet2);
				break;

			case Profession::Revenant:
				/** @var RevenantData */
				$revenantData = $code->ProfessionSpecific;

				if($aquatic) $destination .= pack('x2');
				$destination .= chr($revenantData->Legend1);
				$destination .= chr($revenantData->Legend2);

				if($aquatic) $destination .= pack('x6');
				else $destination .= pack('x2');

				$altSkill1PalletteId = Overrides::RevSkillToPallette($revenantData->AltUtilitySkill1);
				$altSkill2PalletteId = Overrides::RevSkillToPallette($revenantData->AltUtilitySkill2);
				$altSkill3PalletteId = Overrides::RevSkillToPallette($revenantData->AltUtilitySkill3);

				$destination .= pack('v3', $altSkill1PalletteId, $altSkill2PalletteId, $altSkill3PalletteId);

				if(!$aquatic) $destination .= pack('x6');
				break;
		}

		$padding = 44 - strlen($destination);
		if($padding > 0) $destination .= pack("x$padding");

		return $destination;
	}

	#endregion
}