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
use Hardstuck\GuildWars2\BuildCodes\V2\StatId;
use Hardstuck\GuildWars2\BuildCodes\V2\Tests\TestUtilities;
use Hardstuck\GuildWars2\BuildCodes\V2\TextLoader;
use Hardstuck\GuildWars2\BuildCodes\V2\TraitLineChoice;
use Hardstuck\GuildWars2\BuildCodes\V2\Util\StringView;
use Hardstuck\GuildWars2\BuildCodes\V2\Util\TraitLineChoices;
use Hardstuck\GuildWars2\BuildCodes\V2\WeaponType;

use const Hardstuck\GuildWars2\BuildCodes\V2\ALL_EQUIPMENT_COUNT;
use const Hardstuck\GuildWars2\BuildCodes\V2\ALL_INFUSION_COUNT;

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
		$this->expectException(\Exception::class);
		$this->expectErrorMessageMatches('/[Vv]ersion/');
		$code = TextLoader::LoadBuildCode(TestUtilities::$CodesInvalid["wrong-version"]);
	}

	/** @test */
	public function ShouldThrowTooShort()
	{
		$this->expectException(\Exception::class);
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
		for($i = 0; $i < ALL_EQUIPMENT_COUNT; $i++) {
			if($i >= 11 && $i <= 14) $this->assertEquals(StatId::_UNDEFINED, $code->EquipmentAttributes[$i]);
			else if($i === ALL_EQUIPMENT_COUNT - 1) $this->assertEquals(1, $code->EquipmentAttributes[$i]); 
			else $this->assertEquals(StatId::_UNDEFINED, $code->EquipmentAttributes[$i]);
		}
		for($i = 0; $i < ALL_INFUSION_COUNT; $i++)
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

	/** @test */
	public function MidNecro()
	{
		$code = TextLoader::LoadBuildCode(TestUtilities::$CodesV2["mid-necro"]);
		$this->assertEquals(Profession::Necromancer, $code->Profession);

		$this->assertEquals(WeaponType::_UNDEFINED, $code->WeaponSet1->MainHand);
		$this->assertEquals(WeaponType::_UNDEFINED, $code->WeaponSet1->OffHand);
		$this->assertEquals(WeaponType::_UNDEFINED, $code->WeaponSet2->MainHand);
		$this->assertEquals(WeaponType::_UNDEFINED, $code->WeaponSet2->OffHand);

		$this->assertEquals(ItemId::_UNDEFINED, $code->Rune);
		for($i = 0; $i < ALL_EQUIPMENT_COUNT; $i++) {
			$this->assertEquals(StatId::_UNDEFINED, $code->EquipmentAttributes[$i]);
		}

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

	/** @test */
	public function FullNecro()
	{
		$code = TextLoader::LoadBuildCode(TestUtilities::$CodesV2["full-necro"]);
		$this->assertEquals(Profession::Necromancer, $code->Profession);

		$this->assertEquals(WeaponType::Axe    , $code->WeaponSet1->MainHand);
		$this->assertEquals(ItemId::Legendary_Sigil_of_Paralyzation, $code->WeaponSet1->Sigil1);
		$this->assertEquals(WeaponType::Dagger , $code->WeaponSet1->OffHand);
		$this->assertEquals(ItemId::Legendary_Sigil_of_Paralyzation, $code->WeaponSet1->Sigil2);
		$this->assertEquals(WeaponType::Scepter, $code->WeaponSet2->MainHand);
		$this->assertEquals(ItemId::Legendary_Sigil_of_Paralyzation, $code->WeaponSet2->Sigil1);
		$this->assertEquals(WeaponType::Focus  , $code->WeaponSet2->OffHand);
		$this->assertEquals(ItemId::Legendary_Sigil_of_Paralyzation, $code->WeaponSet2->Sigil2);

		$this->assertEquals(ItemId::Superior_Rune_of_the_Scholar, $code->Rune);
		$berserkers = [StatId::Berserkers1, StatId::Berserkers2, StatId::Berserkers3, StatId::Berserkers4, StatId::Berserkers5];
		for($i = 0; $i < ALL_EQUIPMENT_COUNT; $i++) {
			$this->assertContains($code->EquipmentAttributes[$i], $berserkers, "index$i");
		}

		for($i = 0; $i < ALL_INFUSION_COUNT; $i++) {
			$this->assertEquals(ItemId::Mighty_5_Agony_Infusion, $code->Infusions[$i]);
		}

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

		$this->assertEquals(ItemId::Bowl_of_Sweet_and_spicy_Butternut_Squash_Soup, $code->Food);
		$this->assertEquals(ItemId::Tin_of_Fruitcake, $code->Utility);
	}

	public function AllCodesProvider() {
		$arr = [];
		foreach(TestUtilities::$CodesV2 as $name => $code) {
			if(str_contains($name, "binary")) continue;
			array_push($arr, [$name, $code]);
		}
		return $arr;
	}

	/** @test @dataProvider AllCodesProvider */
	public function ParseAll(string $name, string $code)
	{
		$code = TextLoader::LoadBuildCode($code);
		$this->assertTrue(true);
	}

	/** @test @dataProvider AllCodesProvider */
	public function ReencodeAll(string $name, string $code)
	{
		$code_ = TextLoader::LoadBuildCode($code);
		$reencoded = TextLoader::WriteBuildCode($code_);
		$this->assertEquals($code, $reencoded);
	}

	/** @test */
	public function FullNecroCompressed()
	{
		$code = TextLoader::LoadBuildCode(TestUtilities::$CodesV2["full-necro-binary"]);
		$this->assertEquals(Profession::Necromancer, $code->Profession);

		$this->assertEquals(WeaponType::Axe    , $code->WeaponSet1->MainHand);
		$this->assertEquals(ItemId::Legendary_Sigil_of_Paralyzation, $code->WeaponSet1->Sigil1);
		$this->assertEquals(WeaponType::Dagger , $code->WeaponSet1->OffHand);
		$this->assertEquals(ItemId::Legendary_Sigil_of_Paralyzation, $code->WeaponSet1->Sigil2);
		$this->assertEquals(WeaponType::Scepter, $code->WeaponSet2->MainHand);
		$this->assertEquals(ItemId::Legendary_Sigil_of_Paralyzation, $code->WeaponSet2->Sigil1);
		$this->assertEquals(WeaponType::Focus  , $code->WeaponSet2->OffHand);
		$this->assertEquals(ItemId::Legendary_Sigil_of_Paralyzation, $code->WeaponSet2->Sigil2);

		$this->assertEquals(ItemId::Superior_Rune_of_the_Scholar, $code->Rune);
		$berserkers = [StatId::Berserkers1, StatId::Berserkers2, StatId::Berserkers3, StatId::Berserkers4, StatId::Berserkers5];
		for($i = 0; $i < ALL_EQUIPMENT_COUNT; $i++) {
			$this->assertContains($code->EquipmentAttributes[$i], $berserkers, "index$i");
		}

		for($i = 0; $i < ALL_INFUSION_COUNT; $i++) {
			$this->assertEquals(ItemId::Mighty_5_Agony_Infusion, $code->Infusions[$i]);
		}

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

		$this->assertEquals(ItemId::Bowl_of_Sweet_and_spicy_Butternut_Squash_Soup, $code->Food);
		$this->assertEquals(ItemId::Tin_of_Fruitcake, $code->Utility);
	}

	/** @test */ /* regression: mixed infusions w+w/ empty slots in the middle */
	public function Plenyx1()
	{
		$code = new BuildCode();
		$code->EquipmentAttributes->Accessory1  = StatId::Celestial2;
		$code->EquipmentAttributes->Accessory2  = StatId::Celestial2;
		$code->EquipmentAttributes->Amulet      = StatId::Celestial2;
		$code->EquipmentAttributes->BackItem   = StatId::Celestial2;
		$code->EquipmentAttributes->Boots  = StatId::Celestial1;
		$code->EquipmentAttributes->Chest  = StatId::Celestial1;
		$code->EquipmentAttributes->Gloves  = StatId::Celestial1;
		$code->EquipmentAttributes->Helmet  = StatId::Celestial1;
		$code->EquipmentAttributes->Leggings  = StatId::Celestial1;
		$code->EquipmentAttributes->Ring1  = StatId::Celestial2;
		$code->EquipmentAttributes->Ring2  = StatId::Celestial2;
		$code->EquipmentAttributes->Shoulders  = StatId::Celestial1;
		$code->EquipmentAttributes->WeaponSet1MainHand  = StatId::Celestial1;
		$code->EquipmentAttributes->WeaponSet1OffHand  = StatId::_UNDEFINED;
		$code->EquipmentAttributes->WeaponSet2MainHand  = StatId::Celestial1;
		$code->EquipmentAttributes->WeaponSet2OffHand  = StatId::Celestial1;
		$code->Infusions->Helmet  = ItemId::Plus_9_Agony_Infusion;
		$code->Infusions->Shoulders  = ItemId::Plus_9_Agony_Infusion;
		$code->Infusions->Chest  = ItemId::Plus_9_Agony_Infusion;
		$code->Infusions->Gloves  = ItemId::Plus_9_Agony_Infusion;
		$code->Infusions->Leggings  = ItemId::Plus_9_Agony_Infusion;
		$code->Infusions->Boots  = ItemId::Plus_9_Agony_Infusion;
		$code->Infusions->BackItem_1  = ItemId::Plus_9_Agony_Infusion;
		$code->Infusions->BackItem_2  = ItemId::_UNDEFINED;
		$code->Infusions->Accessory1  = ItemId::Expertise_WvW_Infusion;
		$code->Infusions->Accessory2  = ItemId::Expertise_WvW_Infusion;
		$code->Infusions->Ring1_1  = ItemId::Plus_9_Agony_Infusion;
		$code->Infusions->Ring1_2  = ItemId::Plus_9_Agony_Infusion;
		$code->Infusions->Ring1_3  = ItemId::Plus_9_Agony_Infusion;
		$code->Infusions->Ring2_1  = ItemId::Plus_9_Agony_Infusion;
		$code->Infusions->Ring2_2  = ItemId::Plus_9_Agony_Infusion;
		$code->Infusions->Ring2_3  = ItemId::Plus_9_Agony_Infusion;
		$code->Infusions->WeaponSet1_1  = ItemId::_UNDEFINED;
		$code->Infusions->WeaponSet1_2  = ItemId::_UNDEFINED;
		$code->Infusions->WeaponSet2_1  = ItemId::_UNDEFINED;
		$code->Infusions->WeaponSet2_2  = ItemId::_UNDEFINED;
		$code->Infusions->Amulet  = ItemId::WxP_Enrichment;
		$code->Kind  = Kind::PvE;
		$code->Profession  = Profession::Warrior;
		$code->Version  = 3;
		$code->WeaponSet1->MainHand  = WeaponType::Hammer;
		$code->WeaponSet1->OffHand  = WeaponType::_UNDEFINED;
		$code->WeaponSet1->Sigil1  = ItemId::Legendary_Sigil_of_Transference;
		$code->WeaponSet1->Sigil2  = ItemId::Legendary_Sigil_of_Renewal;
		$code->WeaponSet2->MainHand  = WeaponType::Sword;
		$code->WeaponSet2->OffHand  = WeaponType::Warhorn;
		$code->WeaponSet2->Sigil1  = ItemId::Legendary_Sigil_of_Transference;
		$code->WeaponSet2->Sigil2  = ItemId::Legendary_Sigil_of_Energy;


		$text = TextLoader::WriteBuildCode($code);
		$reencode = TextLoader::LoadBuildCode($text);
		$this->assertEquals($code, $reencode);
	}

	/** @test */ /* regression: specific infusion orders break encoding */
	public function Plenyx2()
	{
		$code = new BuildCode();
		$code->EquipmentAttributes->Helmet             = StatId::Berserkers1;
		$code->EquipmentAttributes->Shoulders          = StatId::Berserkers1;
		$code->EquipmentAttributes->Chest              = StatId::Berserkers1;
		$code->EquipmentAttributes->Gloves             = StatId::Berserkers1;
		$code->EquipmentAttributes->Leggings           = StatId::Berserkers1;
		$code->EquipmentAttributes->Boots              = StatId::Berserkers1;
		$code->EquipmentAttributes->BackItem           = StatId::Berserkers2;
		$code->EquipmentAttributes->Accessory1         = StatId::Berserkers2;
		$code->EquipmentAttributes->Accessory2         = StatId::Berserkers2;
		$code->EquipmentAttributes->Ring1              = StatId::Berserkers2;
		$code->EquipmentAttributes->Ring2              = StatId::Berserkers2;
		$code->EquipmentAttributes->WeaponSet1MainHand = StatId::Berserkers1;
		$code->EquipmentAttributes->WeaponSet1OffHand  = StatId::_UNDEFINED;
		$code->EquipmentAttributes->WeaponSet2MainHand = StatId::_UNDEFINED;
		$code->EquipmentAttributes->WeaponSet2OffHand  = StatId::_UNDEFINED;
		$code->EquipmentAttributes->Amulet             = StatId::Berserkers2;
		$code->Infusions->Helmet       = ItemId::Mighty_7_Agony_Infusion;
		$code->Infusions->Shoulders    = ItemId::Precise_7_Agony_Infusion;
		$code->Infusions->Chest        = ItemId::Precise_7_Agony_Infusion;
		$code->Infusions->Gloves       = ItemId::Precise_7_Agony_Infusion;
		$code->Infusions->Leggings     = ItemId::Precise_7_Agony_Infusion;
		$code->Infusions->Boots        = ItemId::Precise_7_Agony_Infusion;
		$code->Infusions->BackItem_1   = ItemId::Concentration_WvW_Infusion;
		$code->Infusions->BackItem_2   = ItemId::_UNDEFINED;
		$code->Infusions->Accessory1   = ItemId::Concentration_WvW_Infusion;
		$code->Infusions->Accessory2   = ItemId::Precise_WvW_Infusion;
		$code->Infusions->Ring1_1      = ItemId::Concentration_WvW_Infusion;
		$code->Infusions->Ring1_2      = ItemId::Concentration_WvW_Infusion;
		$code->Infusions->Ring1_3      = ItemId::Concentration_WvW_Infusion;
		$code->Infusions->Ring2_1      = ItemId::Concentration_WvW_Infusion;
		$code->Infusions->Ring2_2      = ItemId::Concentration_WvW_Infusion;
		$code->Infusions->Ring2_3      = ItemId::Concentration_WvW_Infusion;
		$code->Infusions->WeaponSet1_1 = ItemId::Concentration_WvW_Infusion;
		$code->Infusions->WeaponSet1_2 = ItemId::Concentration_WvW_Infusion;
		$code->Infusions->WeaponSet2_1 = ItemId::_UNDEFINED;
		$code->Infusions->WeaponSet2_2 = ItemId::_UNDEFINED;
		$code->Infusions->Amulet       = ItemId::_UNDEFINED;
		$code->Kind = Kind::PvE;
		$code->Profession = Profession::Engineer;
		$code->Version = 3;
		$code->WeaponSet1->MainHand = WeaponType::Rifle;
		$code->WeaponSet1->OffHand  = WeaponType::_UNDEFINED;
		$code->WeaponSet1->Sigil1   = ItemId::Legendary_Sigil_of_Force;
		$code->WeaponSet1->Sigil2   = ItemId::Legendary_Sigil_of_the_Night;
		$code->WeaponSet2->MainHand = WeaponType::_UNDEFINED;
		$code->WeaponSet2->OffHand  = WeaponType::_UNDEFINED;
		$code->WeaponSet2->Sigil1   = ItemId::_UNDEFINED;
		$code->WeaponSet2->Sigil2   = ItemId::_UNDEFINED;

		$text = TextLoader::WriteBuildCode($code);
		$reencode = TextLoader::LoadBuildCode($text);
		$this->assertEquals($code, $reencode);
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
