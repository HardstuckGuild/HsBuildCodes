<?php namespace Hardstuck\GuildWars2\BuildCodes\V2;

use Hardstuck\GuildWars2\BuildCodes\V2\Util\AllEquipmentInfusions;
use Hardstuck\GuildWars2\BuildCodes\V2\Util\AllEquipmentStats;
use Hardstuck\GuildWars2\BuildCodes\V2\Util\StringView;
use Hardstuck\GuildWars2\BuildCodes\V2\Util\TraitLineChoices;

class TextLoader {
	use Util\_Static;

	public const CHARSET = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-";
	public const INVERSE_CHARSET = [
		/*0x*/ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		/*1x*/ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		/*2x*/ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 62, -1, 63, -1, -1,
		/*3x*/ 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, -1, -1, -1, -1, -1, -1,
		/*4x*/ -1,  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14,
		/*5x*/ 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, -1, -1, -1, -1, -1,
		/*6x*/ -1, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40,
		/*7x*/ 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, -1, -1, -1, -1, -1,
	];

	//TODO(Rennorb): improve decoding for php
	public static function DecodeAndAdvance(StringView $view, int $maxWidth = 1) : int
	{
		if($maxWidth === 1) return TextLoader::INVERSE_CHARSET[$view->NextByte()];

		$value = 0;
		$width = 0;
		do {
			$c = $view->NextChar();
			$mulShift = 6 * $width; // shift by 6, 12, 18 = multiply by 64 64^2 64^3
			$width++;
			if($c === '~') break;
			$value += TextLoader::INVERSE_CHARSET[ord($c)] << $mulShift;
		} while($width < $maxWidth);

		return $value;
	}

	/** Eats the token from view if it is the right one, otherwise does nothing. */
	public static function EatToken(StringView $view, string $token) : bool
	{
		if($view->Data[$view->Pos] === $token) {
			$view->Pos++;
			return true; 
		}
		return false;
	}

	#region hardstuck codes

	public static function LoadBuildCode(string $text) : BuildCode {
		assert(strlen($text) > 10, "Code too short");
		$view = new StringView($text);
		$code = new BuildCode();
		$code->Version    = TextLoader::DecodeAndAdvance($view);
		assert($code->Version === Statics::CURRENT_VERSION, "Code version mismatch");
		$code->Kind       = TextLoader::DecodeAndAdvance($view);
		assert($code->Kind !== Kind::_UNDEFINED, "Code type not valid");
		$code->Profession = Profession::_FIRST() + TextLoader::DecodeAndAdvance($view);

		for($i = 0; $i < 3; $i++) {
			if(!TextLoader::EatToken($view, '_')) {
				$id = TextLoader::DecodeAndAdvance($view);
				$mixed = TextLoader::DecodeAndAdvance($view);
				$choices = new TraitLineChoices();
				for($j = 0; $j < 3; $j++)
					$choices[$j] = ($mixed >> (6 - $j * 2)) & 0b00000011;
				$code->Specializations[$i] = new Specialization($id, $choices);
			}
		}
		if(!TextLoader::EatToken($view, '~')) {
			$code->WeaponSet1 = TextLoader::LoadWeaponSet($view);
			if(!TextLoader::EatToken($view, '~'))
				$code->WeaponSet2 = TextLoader::LoadWeaponSet($view);
		}

		for($i = 0; $i < 5; $i++)
			if(!TextLoader::EatToken($view, '_'))
				$code->SlotSkills[$i] = TextLoader::DecodeAndAdvance($view, 3);
		
		if(!TextLoader::EatToken($view, '_'))
			$code->Rune = TextLoader::DecodeAndAdvance($view, 3);
		
		if($code->Kind !== Kind::PvP)
			$code->EquipmentAttributes = TextLoader::LoadAllEquipmentStats($view, $code);
		else
			$code->EquipmentAttributes->Amulet = TextLoader::DecodeAndAdvance($view, 2);

		if($code->Kind !== Kind::PvP) {
			if(!TextLoader::EatToken($view, '~'))
				$code->Infusions = TextLoader::LoadAllEquipmentInfusions($view, $code);
			if(!TextLoader::EatToken($view, '_'))
				$code->Food = TextLoader::DecodeAndAdvance($view, 3);
			if(!TextLoader::EatToken($view, '_'))
				$code->Utility = TextLoader::DecodeAndAdvance($view, 3);
		}

		$code->ProfessionSpecific = TextLoader::LoadProfessionSpecific($view, $code->Profession);
		$code->Arbitrary          = TextLoader::LoadArbitrary($view);
		return $code;
	}

	private static function LoadWeaponSet(StringView $text) : WeaponSet
	{
		$set = new WeaponSet();
		if(!TextLoader::EatToken($text, '_')) $set->MainHand = WeaponType::_FIRST() + TextLoader::DecodeAndAdvance($text);
		if(!TextLoader::EatToken($text, '_')) $set->Sigil1 = TextLoader::DecodeAndAdvance($text, 3);
		if(!TextLoader::EatToken($text, '_')) $set->OffHand = WeaponType::_FIRST() + TextLoader::DecodeAndAdvance($text);
		if(!TextLoader::EatToken($text, '_')) $set->Sigil2 = TextLoader::DecodeAndAdvance($text, 3);
		return $set;
	}

	private static function LoadAllEquipmentStats(StringView $text, BuildCode $weaponRef) : AllEquipmentStats
	{
		$allData = new AllEquipmentStats();

		$repeatCount = 0;
		$data = StatId::_UNDEFINED;
		for($i = 0; $i < Statics::ALL_EQUIPMENT_COUNT; $i++) {
			if($repeatCount === 0) {
				$data = TextLoader::DecodeAndAdvance($text, 2);

				if($i === Statics::ALL_EQUIPMENT_COUNT - 1) $repeatCount = 1;
				else $repeatCount = TextLoader::DecodeAndAdvance($text);
			}

			switch($i) {
				case 11:
					if(!$weaponRef->WeaponSet1->HasAny()) { $i += 3; continue 2; }
					else if($weaponRef->WeaponSet1->MainHand === WeaponType::_UNDEFINED) { continue 2; }
					else break;
				case 12:
					if($weaponRef->WeaponSet1->OffHand === WeaponType::_UNDEFINED) continue 2;
					else break;
				case 13:
					if(!$weaponRef->WeaponSet2->HasAny()) { $i++; continue 2; }
					else if($weaponRef->WeaponSet2->MainHand === WeaponType::_UNDEFINED) continue 2;
					else break;
				case 14:
					if($weaponRef->WeaponSet2->OffHand === WeaponType::_UNDEFINED) continue 2;
					else break;
			}

			$allData[$i] = $data;
			$repeatCount--;
		}
		return $allData;
	}

	private static function LoadAllEquipmentInfusions(StringView $text, BuildCode $weaponRef) : AllEquipmentInfusions
	{
		$allData = new AllEquipmentInfusions();

		$repeatCount = 0;
		$data = ItemId::_UNDEFINED;
		for($i = 0; $i < Statics::ALL_INFUSION_COUNT; $i++)
		{
			if($repeatCount === 0)
			{
				$data = TextLoader::EatToken($text, '_') ? ItemId::_UNDEFINED : TextLoader::DecodeAndAdvance($text, 3);

				if($i === Statics::ALL_INFUSION_COUNT - 1) $repeatCount = 1;
				else $repeatCount = TextLoader::DecodeAndAdvance($text);
			}

			switch($i) {
				case 16:
					if(!$weaponRef->WeaponSet1->HasAny()) { $i += 3; continue 2; }
					else if($weaponRef->WeaponSet1->MainHand === WeaponType::_UNDEFINED) { continue 2; }
					else break;
				case 17:
					if($weaponRef->WeaponSet1->OffHand === WeaponType::_UNDEFINED) continue 2;
					else break;
				case 18:
					if(!$weaponRef->WeaponSet2->HasAny()) { $i++; continue 2; }
					else if($weaponRef->WeaponSet2->MainHand === WeaponType::_UNDEFINED) continue 2;
					else break;
				case 19:
					if($weaponRef->WeaponSet2->OffHand === WeaponType::_UNDEFINED) continue 2;
					else break;
			}

			$allData[$i] = $data;
			$repeatCount--;
		}
		return $allData;
	}

	private static function LoadProfessionSpecific(StringView $text, int $profession) : IProfessionSpecific
	{
		switch($profession)
		{
			case Profession::Ranger: {
				$data = new RangerData();
				if(!TextLoader::EatToken($text, '~')) {
					if(!TextLoader::EatToken($text, '_'))
						$data->Pet1 = TextLoader::DecodeAndAdvance($text, 2);
					if(!TextLoader::EatToken($text, '_'))
						$data->Pet1 = TextLoader::DecodeAndAdvance($text, 2);
				}
				return $data;
			}

			case Profession::Revenant: {
				$data = new RevenantData();
				$data->Legend1 = TextLoader::DecodeAndAdvance($text) + Legend::_FIRST();
				if(!TextLoader::EatToken($text, '_')) {
					$data->Legend2 = TextLoader::DecodeAndAdvance($text) + Legend::_FIRST();
					if(!TextLoader::EatToken($text, '_'))
						$data->AltUtilitySkill1 = TextLoader::DecodeAndAdvance($text, 3);
					if(!TextLoader::EatToken($text, '_'))
						$data->AltUtilitySkill2 = TextLoader::DecodeAndAdvance($text, 3);
					if(!TextLoader::EatToken($text, '_'))
						$data->AltUtilitySkill3 = TextLoader::DecodeAndAdvance($text, 3);
				}
				return $data;
			}

			default: return ProfessionSpecific\NONE::GetInstance();
		}
	}

	private static function LoadArbitrary(StringView $text) : IArbitrary
	{
		//implement extensions here in the future
		return Arbitrary\NONE::GetInstance();
	}

	//TODO(Rennorb): performance
	public static function EncodeAndAdvance(string &$destination, int $value, int $width) : void
	{
		$pos = 0;
		do
		{
			$destination .= TextLoader::CHARSET[$value & 0b00111111];
			$value >>= 6;
			$pos++;
		} while($value > 0);
		if($pos < $width) $destination .= '~';
	}

	public static function EncodeOrUnderscoreOnZeroAndAdvance(string &$destination, int $value, int $encodeWidth) : void
	{
		if($value === 0) $destination .= '_';
		else TextLoader::EncodeAndAdvance($destination, $value, $encodeWidth); 
	}

	public static function WriteBuildCode(BuildCode $code) : string
	{
		$destination = '';

		$destination .= TextLoader::CHARSET[$code->Version];
		$destination .= TextLoader::CHARSET[$code->Kind];
		$destination .= TextLoader::CHARSET[$code->Profession - 1];
		for($i = 0; $i < 3; $i++) {
			$spec = $code->Specializations[$i];
			if($spec->SpecializationId === SpecializationId::_UNDEFINED) $destination .= '_';
			else {
				$destination .= TextLoader::EncodeAndAdvance($destination, $spec->SpecializationId, 2);
				$destination .= TextLoader::CHARSET[
					($spec->Choices[0] << 4) | ($spec->Choices[1] << 2) | $spec->Choices[2]
				];
			}
		}
		
		if(!$code->WeaponSet1->HasAny()) $destination .= '~';
		else
		{
			if($code->WeaponSet1->MainHand === WeaponType::_UNDEFINED) $destination .= '_';
			else $destination .= TextLoader::CHARSET[$code->WeaponSet1->MainHand - WeaponType::_FIRST()];
			if($code->WeaponSet1->Sigil1 === ItemId::_UNDEFINED) $destination .= '_';
			else TextLoader::EncodeAndAdvance($destination, $code->WeaponSet1->Sigil1, 3);

			if(!$code->WeaponSet2->HasAny()) $destination .= '~';
			else
			{
				if($code->WeaponSet2->MainHand === WeaponType::_UNDEFINED) $destination .= '_';
				else $destination .= TextLoader::CHARSET[$code->WeaponSet2->MainHand - WeaponType::_FIRST()];
				if($code->WeaponSet2->Sigil1 === ItemId::_UNDEFINED) $destination .= '_';
				else TextLoader::EncodeAndAdvance($destination, $code->WeaponSet2->Sigil1, 3);
			}
		}

		for($i = 0; $i < 5; $i++)
			TextLoader::EncodeOrUnderscoreOnZeroAndAdvance($destination, $code->SlotSkills[$i], 3);

		TextLoader::EncodeOrUnderscoreOnZeroAndAdvance($destination, $code->Rune, 3);

		if($code->Kind !== Kind::PvP)TextLoader:: EncodeStatsAndAdvance($destination, $code);
		else TextLoader::EncodeAndAdvance($destination, $code->EquipmentAttributes->Amulet, 2);

		if($code->Kind !== Kind::PvP)
		{
			if(!$code->Infusions->HasAny()) $destination .= '~';
			else TextLoader::EncodeInfusionsAndAdvance($destination, $code);

			TextLoader::EncodeOrUnderscoreOnZeroAndAdvance($destination, $code->Food, 3);
			TextLoader::EncodeOrUnderscoreOnZeroAndAdvance($destination, $code->Utility, 3);
		}

		TextLoader::EncodeProfessionArbitrary($destination, $code->ProfessionSpecific);
		TextLoader::EncodeArbitrary($destination, $code->Arbitrary);

		return $destination;
	}

	private static function EncodeStatsAndAdvance(string &$destination, BuildCode $weaponRef) :  void
	{
		/** @var null|int $lastStat */
		$lastStat = null;
		$repeatCount = 0;
		for($i = 0; $i < Statics::ALL_EQUIPMENT_COUNT; $i++)
		{
			switch($i) {
				case 11:
					if(!$weaponRef->WeaponSet1->HasAny()) { $i += 3; continue 2; }
					else if($weaponRef->WeaponSet1->MainHand === WeaponType::_UNDEFINED) { continue 2; }
					else break;
				case 12:
					if($weaponRef->WeaponSet1->OffHand === WeaponType::_UNDEFINED) continue 2;
					else break;
				case 13:
					if(!$weaponRef->WeaponSet2->HasAny()) { $i++; continue 2; }
					else if($weaponRef->WeaponSet2->MainHand === WeaponType::_UNDEFINED) continue 2;
					else break;
				case 14:
					if($weaponRef->WeaponSet2->OffHand === WeaponType::_UNDEFINED) continue 2;
					else break;
			}

			if($weaponRef->EquipmentAttributes[$i] !== $lastStat)
			{
				if($lastStat !== null)
				{
					TextLoader::EncodeAndAdvance($destination, $lastStat, 2);
					$destination .= TextLoader::CHARSET[$repeatCount];
				}

				$lastStat = $weaponRef->EquipmentAttributes[$i];
				$repeatCount = 1;
			}
			else
			{
				$repeatCount++;
			}
		}

		TextLoader::EncodeAndAdvance($destination, $lastStat, 2);
		if($repeatCount > 1)
			$destination .= TextLoader::CHARSET[$repeatCount];
	}

	private static function EncodeInfusionsAndAdvance(string &$destination, BuildCode $weaponRef) : void
	{
		$lastInfusion = ItemId::_UNDEFINED;
		$repeatCount = 0;
		for($i = 0; $i < Statics::ALL_INFUSION_COUNT; $i++)
		{
			switch($i) {
				case 16:
					if(!$weaponRef->WeaponSet1->HasAny()) { $i += 3; continue 2; }
					else if($weaponRef->WeaponSet1->MainHand === WeaponType::_UNDEFINED) { continue 2; }
					else break;
				case 17:
					if($weaponRef->WeaponSet1->OffHand === WeaponType::_UNDEFINED) continue 2;
					else break;
				case 18:
					if(!$weaponRef->WeaponSet2->HasAny()) { $i++; continue 2; }
					else if($weaponRef->WeaponSet2->MainHand === WeaponType::_UNDEFINED) continue 2;
					else break;
				case 19:
					if($weaponRef->WeaponSet2->OffHand === WeaponType::_UNDEFINED) continue 2;
					else break;
			}

			if($weaponRef->Infusions[$i] !== $lastInfusion)
			{
				if($lastInfusion !== ItemId::_UNDEFINED)
				{
					TextLoader::EncodeAndAdvance($destination, $lastInfusion, 3);
					$destination .= TextLoader::CHARSET[$repeatCount];
				}

				$lastInfusion = $weaponRef->Infusions[$i];
				$repeatCount = 1;
			}
			else
			{
				$repeatCount++;
			}
		}

		TextLoader::EncodeAndAdvance($destination, $lastInfusion, 2);
		if($repeatCount > 1)
			$destination .= TextLoader::CHARSET[$repeatCount];
	}

	private static function EncodeProfessionArbitrary(string &$destination, IProfessionSpecific $professionSpecific) : void
	{
		switch(get_class($professionSpecific))
		{
			case RangerData::class:
				/** @var RangerData $rangerData */
				$rangerData = $professionSpecific;
				if($rangerData->Pet1 === PetId::_UNDEFINED && $rangerData->Pet2 === PetId::_UNDEFINED) $destination .= '~';
				else
				{
					TextLoader::EncodeOrUnderscoreOnZeroAndAdvance($destination, $rangerData->Pet1, 2);
					TextLoader::EncodeOrUnderscoreOnZeroAndAdvance($destination, $rangerData->Pet2, 2);
				}
				break;

			case RevenantData::class:
				/** @var RevenantData $revenantData */
				$revenantData = $professionSpecific;
				$destination .= TextLoader::CHARSET[$revenantData->Legend1 - Legend::_FIRST()];
				if($revenantData->Legend2 === Legend::_UNDEFINED) $destination .= '_';
				else
				{
					$destination .= TextLoader::CHARSET[$revenantData->Legend2 - Legend::_FIRST()];
					TextLoader::EncodeOrUnderscoreOnZeroAndAdvance($destination, $revenantData->AltUtilitySkill1, 3);
					TextLoader::EncodeOrUnderscoreOnZeroAndAdvance($destination, $revenantData->AltUtilitySkill2, 3);
					TextLoader::EncodeOrUnderscoreOnZeroAndAdvance($destination, $revenantData->AltUtilitySkill3, 3);
				}
				break;
		}
	}

	private static function EncodeArbitrary(string &$destination, IArbitrary $arbitraryData) : void
	{
		//space for expansions
		return;
	}

	#endregion

	#region official codes

	/**
	 * @param string $chatLink base64 encoded raw link (without [&...]) or full link (with [&...])
	 * @remarks Requires PerProfessionData to be loaded or PerProfessionData::$LazyLoadMode to be set to something other than LazyLoadMode::NONE.
	 */
	public static function LoadOfficialBuildCode(string $chatLink, bool $aquatic = false) : BuildCode
	{
		$base64 = $chatLink[0] === '[' ? substr($chatLink, 2, strlen($chatLink) - 3) : $chatLink;
		$buffer = base64_decode($base64);
		return BinaryLoader::LoadOfficialBuildCode($buffer, $aquatic);
	}

	/** @remarks Requires PerProfessionData to be loaded or PerProfessionData::$LazyLoadMode to be set to something other than LazyLoadMode::NONE. */
	public static function WriteOfficialBuildCode(BuildCode $code, bool $aquatic = false) : string
	{
		$buffer = BinaryLoader::WriteOfficialBuildCode($code, $aquatic);
		return "[&" . base64_encode($buffer) . ']';
	}

	#endregion
}