<?php namespace Hardstuck\GuildWars2\BuildCodes\V2\Tests\API;

use Hardstuck\GuildWars2\BuildCodes\V2;
use Hardstuck\GuildWars2\BuildCodes\V2\APILoader;
use Hardstuck\GuildWars2\BuildCodes\V2\ItemId;
use Hardstuck\GuildWars2\BuildCodes\V2\Kind;
use Hardstuck\GuildWars2\BuildCodes\V2\Profession;
use Hardstuck\GuildWars2\BuildCodes\V2\SkillId;
use Hardstuck\GuildWars2\BuildCodes\V2\SpecializationId;
use Hardstuck\GuildWars2\BuildCodes\V2\StatId;
use Hardstuck\GuildWars2\BuildCodes\V2\TraitLineChoice;
use Hardstuck\GuildWars2\BuildCodes\V2\Util\TraitLineChoices;
use Hardstuck\GuildWars2\BuildCodes\V2\WeaponType;
use PHPUnit\Framework\TestCase;

use function Hardstuck\GuildWars2\BuildCodes\V2\ResolveAltRevSkills;

class FunctionTests extends TestCase {
	public const VALID_KEY = "92CE5A6C-E594-9D4D-B92B-5621ACFE047D436C02BD-0810-47D9-B9D4-2620EB7DD598";
	public const MISSING_PERMS_KEY = "AD041D99-AEEF-2E45-8732-0057285EFE370740BF1D-6427-4191-8C4F-84DD1C97F05F";

	/** @test */
	public function ShouldThrowNotAToken()
	{
		$this->expectWarning();
		$this->expectWarningMessageMatches('/401/');
		$code = APILoader::LoadBuildCode("xxx", "sss", Kind::PvE);
	}
	
	/** @test */
	public function ShouldThrowInvalidScopes()
	{
		$this->expectWarning();
		$this->expectWarningMessageMatches('/403/');
		$code = APILoader::LoadBuildCode(FunctionTests::MISSING_PERMS_KEY, "sss", Kind::PvE);
	}

	/** @test */
	public function ShouldFindMissingScopes()
	{
		$missingScopes = APILoader::ValidateScopes(FunctionTests::MISSING_PERMS_KEY);
		$this->assertEquals(["characters", "builds"], $missingScopes);
	}

	/** @test */
	public function ShouldThrowNoSuchCharacter()
	{
		$this->expectWarning();
		$this->expectWarningMessageMatches('/404/');
		$code = APILoader::LoadBuildCode(FunctionTests::VALID_KEY, "does not exist", Kind::PvE);
	}
}

class BasicCodesTests extends TestCase {
	/** @test */
	public function LoadBuild()
	{
		$code = APILoader::LoadBuildCode(FunctionTests::VALID_KEY, "Hardstuck Thief", Kind::PvE);
		$this->assertEquals(Profession::Thief, $code->Profession);

		$this->assertEquals(SpecializationId::Deadly_Arts, $code->Specializations[0]->SpecializationId);
		$reference1 = new TraitLineChoices();
		$reference1->Adept       = TraitLineChoice::BOTTOM;
		$reference1->Master      = TraitLineChoice::MIDDLE;
		$reference1->Grandmaster = TraitLineChoice::TOP;
		$this->assertEquals($reference1, $code->Specializations[0]->Choices);

		$this->assertEquals(SpecializationId::Trickery, $code->Specializations[1]->SpecializationId);
		$reference2 = new TraitLineChoices();
		$reference2->Adept       = TraitLineChoice::BOTTOM;
		$reference2->Master      = TraitLineChoice::TOP;
		$reference2->Grandmaster = TraitLineChoice::TOP;
		$this->assertEquals($reference2, $code->Specializations[1]->Choices);

		$this->assertEquals(SpecializationId::Specter, $code->Specializations[2]->SpecializationId);
		$reference3 = new TraitLineChoices();
		$reference3->Adept       = TraitLineChoice::BOTTOM;
		$reference3->Master      = TraitLineChoice::BOTTOM;
		$reference3->Grandmaster = TraitLineChoice::TOP;
		$this->assertEquals($reference3, $code->Specializations[2]->Choices);
		
		$this->assertEquals(WeaponType::Scepter, $code->WeaponSet1->MainHand);
		$this->assertEquals(WeaponType::Dagger , $code->WeaponSet1->OffHand);
		$this->assertEquals(WeaponType::_UNDEFINED, $code->WeaponSet2->MainHand);
		$this->assertEquals(WeaponType::Pistol , $code->WeaponSet2->OffHand);

		$this->assertEquals(ItemId::Legendary_Sigil_of_Demons, $code->WeaponSet1->Sigil1);
		$this->assertEquals(ItemId::Legendary_Sigil_of_Concentration, $code->WeaponSet1->Sigil2);
		$this->assertEquals(ItemId::_UNDEFINED, $code->WeaponSet2->Sigil1);
		$this->assertEquals(ItemId::Legendary_Sigil_of_Paralyzation, $code->WeaponSet2->Sigil2);

		$celestialStatsKEKW = [ StatId::Celestial1, StatId::Celestial2, StatId::Celestial3, StatId::Celestial4 ];
		for($i = 0; $i < V2\ALL_EQUIPMENT_COUNT; $i++)
			if($i !== 13) // empty second main hand
				$this->assertContains($code->EquipmentAttributes[$i], $celestialStatsKEKW);

		$this->assertEquals(SkillId::Well_of_Gloom  , $code->SlotSkills->Heal);
		$this->assertEquals(SkillId::Well_of_Silence, $code->SlotSkills->Utility1);
		$this->assertEquals(SkillId::Well_of_Bounty , $code->SlotSkills->Utility2);
		$this->assertEquals(SkillId::Well_of_Sorrow , $code->SlotSkills->Utility3);
		$this->assertEquals(SkillId::Shadowfall     , $code->SlotSkills->Elite);

		$this->assertEquals(ItemId::Legendary_Rune_of_the_Traveler, $code->Rune);
	}

	/** @test */ /* regression: revenant skills would always show the alliance stance*/
	public function Teapot1()
	{
		$this->markTestSkipped('Teapot keeps changing the build.');

		$code = APILoader::LoadBuildCode(FunctionTests::VALID_KEY, "Hardstuck Revenant", Kind::PvE);
		$altSkills = ResolveAltRevSkills($code->ProfessionSpecific);
		if($code->SlotSkills->Heal != SkillId::Facet_of_Light) {
			$tmp = $code->SlotSkills;
			$code->SlotSkills = $altSkills;
			$altSkills = $tmp;
		}

		$this->assertEquals(SkillId::Facet_of_Light   , $code->SlotSkills->Heal);
		$this->assertEquals(SkillId::Facet_of_Darkness, $code->SlotSkills->Utility1);
		$this->assertEquals(SkillId::Facet_of_Elements, $code->SlotSkills->Utility2);
		$this->assertEquals(SkillId::Facet_of_Strength, $code->SlotSkills->Utility3);
		$this->assertEquals(SkillId::Facet_of_Chaos   , $code->SlotSkills->Elite);

		$this->assertEquals(SkillId::Empowering_Misery   , $altSkills->Heal);
		$this->assertEquals(SkillId::Pain_Absorption     , $altSkills->Utility1);
		$this->assertEquals(SkillId::Banish_Enchantment  , $altSkills->Utility2);
		$this->assertEquals(SkillId::Call_to_Anguish1    , $altSkills->Utility3);
		$this->assertEquals(SkillId::Embrace_the_Darkness, $altSkills->Elite);
	}
}
