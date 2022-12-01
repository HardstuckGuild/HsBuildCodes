<?php namespace Hardstuck\GuildWars2\BuildCodes\V2\Tests\Binary;

require_once 'TestUtilities.php';

use PHPUnit\Framework\TestCase;
use Hardstuck\GuildWars2\BuildCodes\V2;
use Hardstuck\GuildWars2\BuildCodes\V2\BinaryLoader;
use Hardstuck\GuildWars2\BuildCodes\V2\BitReader;
use Hardstuck\GuildWars2\BuildCodes\V2\BitWriter;
use Hardstuck\GuildWars2\BuildCodes\V2\BuildCode;
use Hardstuck\GuildWars2\BuildCodes\V2\ItemId;
use Hardstuck\GuildWars2\BuildCodes\V2\Kind;
use Hardstuck\GuildWars2\BuildCodes\V2\LazyLoadMode;
use Hardstuck\GuildWars2\BuildCodes\V2\Legend;
use Hardstuck\GuildWars2\BuildCodes\V2\PerProfessionData;
use Hardstuck\GuildWars2\BuildCodes\V2\PetId;
use Hardstuck\GuildWars2\BuildCodes\V2\Profession;
use Hardstuck\GuildWars2\BuildCodes\V2\RangerData;
use Hardstuck\GuildWars2\BuildCodes\V2\RevenantData;
use Hardstuck\GuildWars2\BuildCodes\V2\SkillId;
use Hardstuck\GuildWars2\BuildCodes\V2\Specialization;
use Hardstuck\GuildWars2\BuildCodes\V2\SpecializationId;
use Hardstuck\GuildWars2\BuildCodes\V2\StatId;
use Hardstuck\GuildWars2\BuildCodes\V2\Tests\TestUtilities;
use Hardstuck\GuildWars2\BuildCodes\V2\TraitLineChoice;
use Hardstuck\GuildWars2\BuildCodes\V2\Util\TraitLineChoices;
use Hardstuck\GuildWars2\BuildCodes\V2\WeaponType;

use const Hardstuck\GuildWars2\BuildCodes\V2\ALL_EQUIPMENT_COUNT;
use const Hardstuck\GuildWars2\BuildCodes\V2\ALL_INFUSION_COUNT;

set_include_path(__DIR__.'/../../../include/common/');

class FunctionTests extends TestCase {
	/** @test */
	public function DecodeByteValue()
	{
		$data = chr(0b00110011);
		$bitspan = new BitReader($data);
		$this->assertEquals(3, $bitspan->DecodeNext(4));
	}

	/** @test */
	public function SuccessiveDecodeByteValue()
	{
		$data = chr(0b0011_1100);
		$bitspan = new BitReader($data);
		$this->assertEquals( 3, $bitspan->DecodeNext(4));
		$this->assertEquals(12, $bitspan->DecodeNext(4));
	}

	/** @test */
	public function DecodeMultibyteValue()
	{
		$data = chr(0b0000_0000).chr(0b0001_0000);
		$bitspan = new BitReader($data);
		$this->assertEquals(1, $bitspan->DecodeNext(12));

		$data2 = chr(0b1000_0000).chr(0b0000_0000);
		$bitspan2 = new BitReader($data2);
		$this->assertEquals(0b1000_0000_0000, $bitspan2->DecodeNext(12));
	}

	/** @test */
	public function DecodeMultibyteValue2()
	{
		$data = pack('C*', 0b0000_0000, 0b0000_0000, 0b0000_0000, 0b0000_0010);
		$bitspan = new BitReader($data);
		$bitspan->BitPos = 7;
		$this->assertEquals(1, $bitspan->DecodeNext(24));
	}

	/** @test */
	public function SuccessiveDecodeMultibyteValue()
	{
		$data = chr(0b00110011).chr(0b00110001);
		$bitspan = new BitReader($data);
		$this->assertEquals(0b001100110011, $bitspan->DecodeNext(12));
		$this->assertEquals(1, $bitspan->DecodeNext(4));
	}

	/** @test */
	public function SuccessiveDecodeValueCrossByteBoundary()
	{
		$data = pack('C*', 0b01010101, 0b10101010, 0b11110000 );
		$bitspan = new BitReader($data);
		$this->assertEquals(0b010101011010, $bitspan->DecodeNext(12));
		$this->assertEquals(0b101011, $bitspan->DecodeNext(6));
	}

	/** @test */
	public function EatIfExpected()
	{
		$data = pack('C*', 0b00000011, 0b00110001 );
		$bitspan = new BitReader($data);
		$this->assertFalse($bitspan->EatIfExpected(8, 5));
		$this->assertEquals(0, $bitspan->BitPos);
		$this->assertTrue($bitspan->EatIfExpected(0, 5));
		$this->assertEquals(5, $bitspan->BitPos);
	}

	/** @test */
	public function WriteBits()
	{
		$bitStream = new BitWriter();
		$bitStream->Write(3, 2);
		$this->assertEquals(0b11000000, $bitStream->Data[0]);
	}

	/** @test */
	public function WriteManyBits()
	{
		$bitStream = new BitWriter();
		$bitStream->Write(3, 24);
		$this->assertEquals([0, 0, 0b00000011], $bitStream->Data);
	}

	/** @test */
	public function SuccessiveWriteBits()
	{
		$bitStream = new BitWriter();
		$bitStream->Write(3, 2);
		$bitStream->Write(3, 2);
		$this->assertEquals(0b11110000, $bitStream->Data[0]);
	}

	/** @test */
	public function SuccessiveWriteBitsAcrossByteBoundry()
	{
		$bitStream = new BitWriter();
		$bitStream->Write(3, 6);
		$bitStream->Write(3, 4);
		$this->assertEquals(0b00001100, $bitStream->Data[0]);
		$this->assertEquals(0b11000000, $bitStream->Data[1]);
	}

	/** @test */
	public function SuccessiveWriteManyBits()
	{
		$bitStream = new BitWriter();
		$bitStream->Write(3, 20);
		$bitStream->Write(3, 24);
		$this->assertEquals([0, 0, 0b00110000, 0, 0, 0b00110000], $bitStream->Data);
	}
}

class BasicCodeTests extends TestCase {
	static function BitStringToBytes(string $data) : string {
		$list  = '';
		$counter = 0;
		$current = 0;
		foreach(str_split($data) as $c) {
			switch($c)
			{
				case '0':
					$current <<= 1;
					$counter++;
					break;

				case '1':
					$current <<= 1;
					$current |= 1;
					$counter++;
					break;

				case $c >= 'a' && $c <= 'z' || ($c >= 'A' && $c <= 'Z'):
					if($counter !== 0) assert(false, "only on byte boundries");
					$list .= $c;
					break;

			}

			if($counter === 8)
			{
				$list .= chr($current);
				$current = 0;
				$counter = 0;
			}
		}

		if($counter > 0) {
			$list .= chr(($current << (8 - $counter)) & 0xFF);
		}

		return $list;
	}

	public function TrueFalseProvider() : array
	{ return [[true], [false]]; }

	/** @test */
	public function HelperWorking()
	{
		$rawCode0 = BasicCodeTests::BitStringToBytes("b01010101");
		$this->assertEquals('b'.chr(0b01010101), $rawCode0);

		$rawCode1 = BasicCodeTests::BitStringToBytes("b010001");
		$this->assertEquals('b'.chr(0b01000100), $rawCode1);

		$rawCode2 = BasicCodeTests::BitStringToBytes("b0100_01");
		$this->assertEquals('b'.chr(0b01000100), $rawCode2);
	}

	/** @test */
	public function ShouldThrowVersion()
	{
		$rawCode = 'B'.str_repeat(chr(0x2), 79);

		$this->expectException(\Exception::class);
		$this->expectErrorMessageMatches('/[Vv]ersion/');
		$code = BinaryLoader::LoadBuildCode($rawCode);
	}

	/** @test */
	public function ShouldThrowTooShort()
	{
		$rawCode = 'd'.chr(0x2);
		$this->expectWarning();
		$this->expectWarningMessage("Uninitialized string offset 2");
		$code = BinaryLoader::LoadBuildCode($rawCode);
	}

	/** @test @dataProvider TrueFalseProvider  */
	public function MinimalPvPWithSkills(bool $lazyload)
	{
		if($lazyload) PerProfessionData::$LazyLoadMode = LazyLoadMode::OFFLINE_ONLY;
		else PerProfessionData::Reload(Profession::Guardian, true);

		$rawCode = BasicCodeTests::BitStringToBytes(TestUtilities::$CodesV2Binary["minimal-pvp-with-skills"]);
		$code = BinaryLoader::LoadBuildCode($rawCode);
		$this->assertEquals(3                   , $code->Version);
		$this->assertEquals(Kind::PvP           , $code->Kind);
		$this->assertEquals(Profession::Guardian, $code->Profession);
		for($i = 0; $i < 3; $i++)
			$this->assertEquals(SpecializationId::_UNDEFINED, $code->Specializations[$i]->SpecializationId);
		$this->assertFalse($code->WeaponSet1->HasAny());
		$this->assertFalse($code->WeaponSet2->HasAny());
		for($i = 0; $i < 5; $i++)
			$this->assertEquals($i + 1, $code->SlotSkills[$i]);
		$this->assertEquals(ItemId::_UNDEFINED, $code->Rune);
		for($i = 0; $i < ALL_EQUIPMENT_COUNT; $i++) {
			if($i >= 11 && $i <= 14) $this->assertEquals(StatId::_UNDEFINED, $code->EquipmentAttributes[$i]);
			else if($i === ALL_EQUIPMENT_COUNT - 1)  $this->assertEquals(1, $code->EquipmentAttributes[$i]);
			else $this->assertEquals(StatId::_UNDEFINED, $code->EquipmentAttributes[$i]);
		}
		for($i = 0; $i < ALL_INFUSION_COUNT; $i++)
			$this->assertEquals(ItemId::_UNDEFINED, $code->Infusions[$i]);
		$this->assertEquals(ItemId::_UNDEFINED, $code->Food);
		$this->assertEquals(ItemId::_UNDEFINED, $code->Utility);
		$this->assertEquals(V2\ProfessionSpecific\NONE::GetInstance(), $code->ProfessionSpecific);
		$this->assertEquals(         V2\Arbitrary\NONE::GetInstance(), $code->Arbitrary);
	}

	/** @test @dataProvider TrueFalseProvider  */
	public function MinimalPvE(bool $lazyload)
	{
		if($lazyload) PerProfessionData::$LazyLoadMode = LazyLoadMode::OFFLINE_ONLY;
		else PerProfessionData::Reload(Profession::Guardian, true);

		$rawCode = BasicCodeTests::BitStringToBytes(TestUtilities::$CodesV2Binary["minimal-pve"]);
		$code = BinaryLoader::LoadBuildCode($rawCode);
		$this->assertEquals(3                   , $code->Version);
		$this->assertEquals(Kind::PvE           , $code->Kind);
		$this->assertEquals(Profession::Guardian, $code->Profession);
		for($i = 0; $i < 3; $i++)
			$this->assertEquals(SpecializationId::_UNDEFINED, $code->Specializations[$i]->SpecializationId);
		$this->assertFalse($code->WeaponSet1->HasAny());
		$this->assertFalse($code->WeaponSet2->HasAny());
		for($i = 0; $i < 5; $i++)
			$this->assertEquals(SkillId::_UNDEFINED, $code->SlotSkills[$i]);
		$this->assertEquals(ItemId::_UNDEFINED, $code->Rune);
		for($i = 0; $i < ALL_EQUIPMENT_COUNT; $i++) {
			if(11 <= $i && $i <= 14) $this->assertEquals(StatId::_UNDEFINED, $code->EquipmentAttributes[$i]);
			else $this->assertEquals(1, $code->EquipmentAttributes[$i]);
		}
		for($i = 0; $i < ALL_INFUSION_COUNT; $i++)
			$this->assertEquals(ItemId::_UNDEFINED, $code->Infusions[$i]);
		$this->assertEquals(ItemId::_UNDEFINED, $code->Food);
		$this->assertEquals(ItemId::_UNDEFINED, $code->Utility);
		$this->assertEquals(V2\ProfessionSpecific\NONE::GetInstance(), $code->ProfessionSpecific);
		$this->assertEquals(         V2\Arbitrary\NONE::GetInstance(), $code->Arbitrary);
	}

	/** @test @dataProvider TrueFalseProvider  */
	public function MinimalRanger(bool $lazyload)
	{
		if($lazyload) PerProfessionData::$LazyLoadMode = LazyLoadMode::OFFLINE_ONLY;
		else PerProfessionData::Reload(Profession::Ranger, true);

		$rawCode = BasicCodeTests::BitStringToBytes(TestUtilities::$CodesV2Binary["minimal-ranger"]);
		$code = BinaryLoader::LoadBuildCode($rawCode);
		$this->assertEquals(Profession::Ranger, $code->Profession);
		$this->assertInstanceOf(RangerData::class, $code->ProfessionSpecific);
		$data = $code->ProfessionSpecific;
		$this->assertEquals(PetId::_UNDEFINED, $data->Pet1);
		$this->assertEquals(PetId::_UNDEFINED, $data->Pet2);
	}

	/** @test @dataProvider TrueFalseProvider  */
	public function MinimalRevenant(bool $lazyload)
	{
		if($lazyload) PerProfessionData::$LazyLoadMode = LazyLoadMode::OFFLINE_ONLY;
		else PerProfessionData::Reload(Profession::Revenant, true);

		$rawCode = BasicCodeTests::BitStringToBytes(TestUtilities::$CodesV2Binary["minimal-revenant"]);
		$code = BinaryLoader::LoadBuildCode($rawCode);
		$this->assertEquals(Profession::Revenant, $code->Profession);
		$this->assertInstanceOf(RevenantData::class, $code->ProfessionSpecific);
		$data = $code->ProfessionSpecific;
		$this->assertEquals(Legend::SHIRO      , $data->Legend1);
		$this->assertEquals(Legend::_UNDEFINED , $data->Legend2);
		$this->assertEquals(SkillId::_UNDEFINED, $data->AltUtilitySkill1);
		$this->assertEquals(SkillId::_UNDEFINED, $data->AltUtilitySkill2);
		$this->assertEquals(SkillId::_UNDEFINED, $data->AltUtilitySkill3);
	}

	/** @test @dataProvider TrueFalseProvider  */
	public function LoopWriteMinimalRevenant(bool $lazyload)
	{
		if($lazyload) PerProfessionData::$LazyLoadMode = LazyLoadMode::OFFLINE_ONLY;
		else PerProfessionData::Reload(Profession::Revenant, true);

		$rawCode = BasicCodeTests::BitStringToBytes(TestUtilities::$CodesV2Binary["minimal-revenant"]);
		$code = BinaryLoader::LoadBuildCode($rawCode);

		$result = BinaryLoader::WriteBuildCode($code);

		$this->assertEquals($rawCode, $result);
	}
}

class OfficialChatLinks extends TestCase
{
	public function TrueFalseProvider() : array
	{ return [[true], [false]]; }
	
	/** @test @dataProvider TrueFalseProvider  */
	public function LoadOfficialLink(bool $lazyload)
	{
		if($lazyload) PerProfessionData::$LazyLoadMode = LazyLoadMode::OFFLINE_ONLY;
		else PerProfessionData::Reload(Profession::Necromancer, true);

		$fullLink = TestUtilities::$CodesIngame["full-necro"];
		$base64   = substr($fullLink, 2, strlen($fullLink) - 3);
		$raw = base64_decode($base64);
		$code = BinaryLoader::LoadOfficialBuildCode($raw);
		$this->assertNotEquals(Kind::_UNDEFINED, $code->Kind);
		$this->assertEquals(Profession::Necromancer, $code->Profession);

		$this->assertEquals(WeaponType::_UNDEFINED, $code->WeaponSet1->MainHand);
		$this->assertEquals(WeaponType::_UNDEFINED, $code->WeaponSet1->OffHand);
		$this->assertEquals(WeaponType::_UNDEFINED, $code->WeaponSet2->MainHand);
		$this->assertEquals(WeaponType::_UNDEFINED, $code->WeaponSet2->OffHand);

		$this->assertEquals(SpecializationId::Spite, $code->Specializations[0]->SpecializationId);
		$reference1 = new TraitLineChoices();
		$reference1->Adept       = TraitLineChoice::TOP;
		$reference1->Master      = TraitLineChoice::MIDDLE;
		$reference1->Grandmaster = TraitLineChoice::MIDDLE;
		$this->assertEquals($reference1, $code->Specializations[0]->Choices);

		$this->assertEquals(SpecializationId::Soul_Reaping, $code->Specializations[1]->SpecializationId);
		$reference2 = new TraitLineChoices();
		$reference2->Adept       = TraitLineChoice::TOP;
		$reference2->Master      = TraitLineChoice::TOP;
		$reference2->Grandmaster = TraitLineChoice::MIDDLE;
		$this->assertEquals($reference2, $code->Specializations[1]->Choices);

		$this->assertEquals(SpecializationId::Reaper, $code->Specializations[2]->SpecializationId);
		$reference3 = new TraitLineChoices();
		$reference3->Adept       = TraitLineChoice::MIDDLE;
		$reference3->Master      = TraitLineChoice::TOP;
		$reference3->Grandmaster = TraitLineChoice::BOTTOM;
		$this->assertEquals($reference3, $code->Specializations[2]->Choices);

		$this->assertEquals(SkillId::Your_Soul_Is_Mine, $code->SlotSkills[0]);
		$this->assertEquals(SkillId::Well_of_Suffering1, $code->SlotSkills[1]);
		$this->assertEquals(SkillId::Well_of_Darkness1, $code->SlotSkills[2]);
		$this->assertEquals(SkillId::Signet_of_Spite, $code->SlotSkills[3]);
		$this->assertEquals(SkillId::Summon_Flesh_Golem, $code->SlotSkills[4]);
	}

	/** @test @dataProvider TrueFalseProvider  */
	public function WriteOfficialLink(bool $lazyload)
	{
		$code = new BuildCode();
		$code->Profession = Profession::Necromancer;
		$choices1 = new TraitLineChoices();
		$choices1->Adept       = TraitLineChoice::TOP;
		$choices1->Master      = TraitLineChoice::MIDDLE;
		$choices1->Grandmaster = TraitLineChoice::MIDDLE;
		$code->Specializations->Choice1 = new Specialization(SpecializationId::Spite, $choices1);
		$choices2 = new TraitLineChoices();
		$choices2->Adept       = TraitLineChoice::TOP;
		$choices2->Master      = TraitLineChoice::TOP;
		$choices2->Grandmaster = TraitLineChoice::MIDDLE;
		$code->Specializations->Choice2 = new Specialization(SpecializationId::Soul_Reaping, $choices2);
		$choices3 = new TraitLineChoices();
		$choices3->Adept       = TraitLineChoice::MIDDLE;
		$choices3->Master      = TraitLineChoice::TOP;
		$choices3->Grandmaster = TraitLineChoice::BOTTOM;
		$code->Specializations->Choice3 = new Specialization(SpecializationId::Reaper, $choices3);
		
		$code->SlotSkills->Heal     = SkillId::Your_Soul_Is_Mine;
		$code->SlotSkills->Utility1 = SkillId::Well_of_Suffering1;
		$code->SlotSkills->Utility2 = SkillId::Well_of_Darkness1;
		$code->SlotSkills->Utility3 = SkillId::Signet_of_Spite;
		$code->SlotSkills->Elite    = SkillId::Summon_Flesh_Golem;

		if($lazyload) PerProfessionData::$LazyLoadMode = LazyLoadMode::OFFLINE_ONLY;
		else PerProfessionData::Reload(Profession::Necromancer, true);

		$buffer = BinaryLoader::WriteOfficialBuildCode($code);

		$reference = TestUtilities::$CodesIngame["full-necro2"];
		$referenceBase64 = substr($reference, 2, strlen($reference) - 3);
		$referenceBytes = base64_decode($referenceBase64);

		$this->assertEquals($referenceBytes, $buffer);
	}

	/** @test  @dataProvider TrueFalseProvider */
	public function LoadOfficiaRevlLink(bool $lazyload) // our very special boy spec
	{
		if($lazyload) PerProfessionData::$LazyLoadMode = LazyLoadMode::OFFLINE_ONLY;
		else PerProfessionData::Reload(Profession::Revenant, true);

		$fullLink = TestUtilities::$CodesIngame["partial-revenant"];
		$base64   = substr($fullLink, 2, strlen($fullLink) - 3);
		$raw = base64_decode($base64);
		$code = BinaryLoader::LoadOfficialBuildCode($raw);
		$this->assertEquals(Profession::Revenant, $code->Profession);
		$this->assertEquals(SkillId::Empowering_Misery, $code->SlotSkills[0]);
		$this->assertEquals(SkillId::_UNDEFINED, $code->SlotSkills[1]);
		$this->assertEquals(SkillId::Banish_Enchantment, $code->SlotSkills[2]);
		$this->assertEquals(SkillId::Call_to_Anguish1, $code->SlotSkills[3]);
		$this->assertEquals(SkillId::_UNDEFINED, $code->SlotSkills[4]);
	}
}

