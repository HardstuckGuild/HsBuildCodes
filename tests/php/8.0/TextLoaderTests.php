<?php namespace Hardstuck\GuildWars2\BuildCodes\V2\Tests\Text;

require_once 'TestUtilities.php';

use PHPUnit\Framework\TestCase;
use Hardstuck\GuildWars2\BuildCodes\V2;
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
use Hardstuck\GuildWars2\BuildCodes\V2\Statics;
use Hardstuck\GuildWars2\BuildCodes\V2\StatId;
use Hardstuck\GuildWars2\BuildCodes\V2\Tests\TestUtilities;
use Hardstuck\GuildWars2\BuildCodes\V2\TextLoader;
use Hardstuck\GuildWars2\BuildCodes\V2\TraitLineChoice;
use Hardstuck\GuildWars2\BuildCodes\V2\Util\StringView;
use Hardstuck\GuildWars2\BuildCodes\V2\Util\TraitLineChoices;

set_include_path(__DIR__.'/../../../include/common/');

class FunctionTests extends TestCase {

	/** @test */
	public function DecodeValueFixed()
	{
		for($i = 0; $i < 64; $i++)
			$this->assertEquals($i, TextLoader::INVERSE_CHARSET[ord(TextLoader::CHARSET[$i])]);
	}

	/** @test */
	public function SuccessiveDecodeAndEatValueFixed()
	{
		$text = new StringView("Aa-");
		$this->assertEquals( 0, TextLoader::DecodeAndAdvance($text));
		$this->assertEquals(26, TextLoader::DecodeAndAdvance($text));
		$this->assertEquals(63, TextLoader::DecodeAndAdvance($text));
		$this->assertEquals(0, $text->LengthRemaining());
	}

	/** @test */
	public function SuccessiveDecodeAndEatValueValirable()
	{
		$text = new StringView("Aa-");
		$this->assertEquals( 0, TextLoader::DecodeAndAdvance($text, 1));
		$this->assertEquals(26, TextLoader::DecodeAndAdvance($text, 1));
		$this->assertEquals(63, TextLoader::DecodeAndAdvance($text, 1));
		$this->assertEquals(0, $text->LengthRemaining());
	}

	/** @test */
	public function DecodeAndEatValueEarlyTerm()
	{
		$text = new StringView("A~");
		$this->assertEquals(0, TextLoader::DecodeAndAdvance($text, 3));
		$this->assertEquals(0, $text->LengthRemaining());
	}
}

class BasicCodesTests extends TestCase {
	/** @test */
	public function ShouldThrowVersion()
	{
		$this->expectException(\AssertionError::class);
		$this->expectErrorMessageMatches('/[Vv]ersion/');
		$code = TextLoader::LoadBuildCode(TestUtilities::$CodesInvalid["wrong-version"]);
	}

	/** @test */
	public function ShouldThrowTooShort()
	{
		$this->expectException(\AssertionError::class);
		$this->expectErrorMessageMatches('/[Ss]hort/');
		$code = TextLoader::LoadBuildCode(TestUtilities::$CodesInvalid["too-short"]);
	}

	/** @test */
	public function ShouldThrowInvalidCharacters()
	{
		$this->expectWarning();
		$this->expectWarningMessage("Undefined array key 239");
		$code = TextLoader::LoadBuildCode(TestUtilities::$CodesInvalid["invalid-chars"]);
	}

	/** @test */
	public function MinimalPvP()
	{
		$code = TextLoader::LoadBuildCode(TestUtilities::$CodesV2["minimal-pvp"]);
		$this->assertEquals(3                   , $code->Version);
		$this->assertEquals(Kind::PvP           , $code->Kind);
		$this->assertEquals(Profession::Guardian, $code->Profession);
		for($i = 0; $i < 3; $i++)
			$this->assertEquals(SpecializationId::_UNDEFINED, $code->Specializations[$i]->SpecializationId);
		$this->assertFalse($code->WeaponSet1->HasAny());
		$this->assertFalse($code->WeaponSet2->HasAny());
		for($i = 0; $i < 5; $i++)
			$this->assertEquals(SkillId::_UNDEFINED, $code->SlotSkills[$i]);
		$this->assertEquals(ItemId::_UNDEFINED, $code->Rune);
		for($i = 0; $i < Statics::ALL_EQUIPMENT_COUNT; $i++) {
			if($i >= 11 && $i <= 14) $this->assertEquals(StatId::_UNDEFINED, $code->EquipmentAttributes[$i]);
			else if($i === Statics::ALL_EQUIPMENT_COUNT - 1) $this->assertEquals(1, $code->EquipmentAttributes[$i]); 
			else $this->assertEquals(StatId::_UNDEFINED, $code->EquipmentAttributes[$i]);
		}
		for($i = 0; $i < Statics::ALL_INFUSION_COUNT; $i++)
			$this->assertEquals(ItemId::_UNDEFINED, $code->Infusions[$i]);
		$this->assertEquals(ItemId::_UNDEFINED, $code->Food);
		$this->assertEquals(ItemId::_UNDEFINED, $code->Utility);
		$this->assertEquals(V2\ProfessionSpecific\NONE::GetInstance(), $code->ProfessionSpecific);
		$this->assertEquals(         V2\Arbitrary\NONE::GetInstance(), $code->Arbitrary);
	}

	/** @test */
	public function MinimalPvE()
	{
		$code = TextLoader::LoadBuildCode(TestUtilities::$CodesV2["minimal-pve"]);
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
		for($i = 0; $i < Statics::ALL_EQUIPMENT_COUNT; $i++) {
			if(11 <= $i && $i <= 14) $this->assertEquals(StatId::_UNDEFINED, $code->EquipmentAttributes[$i]);
			else $this->assertEquals(1, $code->EquipmentAttributes[$i]);
		}
		for($i = 0; $i < Statics::ALL_INFUSION_COUNT; $i++)
			$this->assertEquals(ItemId::_UNDEFINED, $code->Infusions[$i]);
		$this->assertEquals(ItemId::_UNDEFINED, $code->Food);
		$this->assertEquals(ItemId::_UNDEFINED, $code->Utility);
		$this->assertEquals(V2\ProfessionSpecific\NONE::GetInstance(), $code->ProfessionSpecific);
		$this->assertEquals(         V2\Arbitrary\NONE::GetInstance(), $code->Arbitrary);
	}

	/** @test */
	public function MinimalRanger()
	{
		$code = TextLoader::LoadBuildCode(TestUtilities::$CodesV2["minimal-ranger"]);
		$this->assertInstanceOf(RangerData::class, $code->ProfessionSpecific);
		/** @var RangerData */
		$data = $code->ProfessionSpecific;
		$this->assertEquals(PetId::_UNDEFINED, $data->Pet1);
		$this->assertEquals(PetId::_UNDEFINED, $data->Pet2);
	}

	/** @test */
	public function MinimalRevenant()
	{
		$code = TextLoader::LoadBuildCode(TestUtilities::$CodesV2["minimal-revenant"]);
		$this->assertInstanceOf(RevenantData::class, $code->ProfessionSpecific);
		/** @var RevenantData */
		$data = $code->ProfessionSpecific;
		$this->assertEquals(Legend::SHIRO, $data->Legend1);
		$this->assertEquals(Legend::_UNDEFINED, $data->Legend2);
		$this->assertEquals(SkillId::_UNDEFINED, $data->AltUtilitySkill1);
		$this->assertEquals(SkillId::_UNDEFINED, $data->AltUtilitySkill2);
		$this->assertEquals(SkillId::_UNDEFINED, $data->AltUtilitySkill3);
	}

	/** @test */
	public function CycleBasicCode()
	{
		$text1 = TestUtilities::$CodesV2["minimal-revenant"];
		$code = TextLoader::LoadBuildCode($text1);
		$text2 = TextLoader::WriteBuildCode($code);
		$this->assertEquals($text1, $text2);
	}
}

class OfficialChatLinks extends TestCase {
	public function TrueFalseProvider() : array
	{ return [[true], [false]]; }

	/** @test @dataProvider TrueFalseProvider */
	public function LoadOfficialLink(bool $lazyload)
	{
		if($lazyload) PerProfessionData::$LazyLoadMode = LazyLoadMode::OFFLINE_ONLY;
		else PerProfessionData::Reload(Profession::Necromancer, true);

		$code = TextLoader::LoadOfficialBuildCode(TestUtilities::$CodesIngame["full-necro"]);
		$this->assertEquals(Profession::Necromancer, $code->Profession);

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

	/** @test @dataProvider TrueFalseProvider */
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

		$reference = TestUtilities::$CodesIngame["full-necro2"];
		$result = TextLoader::WriteOfficialBuildCode($code);
		$this->assertEquals($reference, $result);
	}
}
