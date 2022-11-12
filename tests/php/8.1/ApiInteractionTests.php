<?php namespace Hardstuck\GuildWars2\BuildCodes\V2\Tests\APICache;

use Hardstuck\GuildWars2\BuildCodes\V2\APICache;
use Hardstuck\GuildWars2\BuildCodes\V2\BuildCode;
use Hardstuck\GuildWars2\BuildCodes\V2\ItemId;
use Hardstuck\GuildWars2\BuildCodes\V2\Profession;
use Hardstuck\GuildWars2\BuildCodes\V2\SkillId;
use Hardstuck\GuildWars2\BuildCodes\V2\SpecializationId;
use Hardstuck\GuildWars2\BuildCodes\V2\Statics;
use Hardstuck\GuildWars2\BuildCodes\V2\TraitId;
use Hardstuck\GuildWars2\BuildCodes\V2\TraitLineChoice;
use Hardstuck\GuildWars2\BuildCodes\V2\TraitSlot;
use Hardstuck\GuildWars2\BuildCodes\V2\WeaponSetNumber;
use Hardstuck\GuildWars2\BuildCodes\V2\WeaponType;
use PHPUnit\Framework\TestCase;

set_include_path(__DIR__.'/../../../include/common/');

class ResolveWeaponSkills extends TestCase {
	/** @test */
	public function ResolveWeaponSkillsEmpty() {
		$code = new BuildCode();

		$effective = Statics::ResolveEffectiveWeapons($code, WeaponSetNumber::Set1);

		$this->assertEquals(WeaponType::_UNDEFINED, $effective->MainHand);
		$this->assertEquals(WeaponType::_UNDEFINED, $effective->OffHand);

		$reference = array_fill(0, 5, SkillId::_UNDEFINED);

		for($i = 0; $i < count($reference); $i++)
			$this->assertEquals($reference[$i], APICache::ResolveWeaponSkill($code, $effective, $i));
	}
	
	/** @test */
	public function ResolveWeaponSkills2h() {
		$code = new BuildCode();
		$code->Profession = Profession::Necromancer;
		$code->WeaponSet1->MainHand = WeaponType::Staff;

		$effective = Statics::ResolveEffectiveWeapons($code, WeaponSetNumber::Set1);

		$this->assertEquals(WeaponType::Staff, $effective->MainHand);
		$this->assertEquals(WeaponType::_UNDEFINED, $effective->OffHand);

		$reference = [ SkillId::Necrotic_Grasp, SkillId::Mark_of_Blood, SkillId::Chillblains, SkillId::Putrid_Mark, SkillId::Reapers_Mark ];

		for($i = 0; $i < count($reference); $i++)
			$this->assertEquals($reference[$i], APICache::ResolveWeaponSkill($code, $effective, $i));
	}

	/** @test */
	public function ResolveWeaponSkillsNormal() {
		$code = new BuildCode();
		$code->Profession = Profession::Necromancer;
		$code->WeaponSet1->MainHand = WeaponType::Dagger;
		$code->WeaponSet1->OffHand  = WeaponType::Dagger;

		$effective = Statics::ResolveEffectiveWeapons($code, WeaponSetNumber::Set1);

		$this->assertEquals(WeaponType::Dagger, $effective->MainHand);
		$this->assertEquals(WeaponType::Dagger, $effective->OffHand);

		$reference = [ SkillId::Necrotic_Slash, SkillId::Life_Siphon, SkillId::Dark_Pact, SkillId::Deathly_Swarm, SkillId::Enfeebling_Blood ];

		for($i = 0; $i < count($reference); $i++)
			$this->assertEquals($reference[$i], APICache::ResolveWeaponSkill($code, $effective, $i));
	}

	/** @test */
	public function ResolveWeaponSkillsFromOtherSet()
	{
		$code = new BuildCode();
		$code->Profession = Profession::Necromancer;
		$code->WeaponSet1->MainHand = WeaponType::Dagger;
		$code->WeaponSet1->Sigil1 = ItemId::Superior_Sigil_of_Deamons2;
		$code->WeaponSet2->OffHand  = WeaponType::Dagger;
		$code->WeaponSet2->Sigil2 = ItemId::Superior_Sigil_of_Concentration2;

		$effective = Statics::ResolveEffectiveWeapons($code, WeaponSetNumber::Set1);

		$this->assertEquals(WeaponType::Dagger, $effective->MainHand);
		$this->assertEquals(ItemId::Superior_Sigil_of_Deamons2, $effective->Sigil1);
		$this->assertEquals(WeaponType::Dagger, $effective->OffHand);
		$this->assertEquals(ItemId::Superior_Sigil_of_Concentration2, $effective->Sigil2);


		$reference = [ SkillId::Necrotic_Slash, SkillId::Life_Siphon, SkillId::Dark_Pact, SkillId::Deathly_Swarm, SkillId::Enfeebling_Blood ];

		for($i = 0; $i < count($reference); $i++)
			$this->assertEquals($reference[$i], APICache::ResolveWeaponSkill($code, $effective, $i));
	}

	/** @test */
	public function ResolveWeaponSkillsFromOtherSetExcept2h()
	{
		$code = new BuildCode();
		$code->Profession = Profession::Necromancer;
		$code->WeaponSet1->MainHand = WeaponType::Dagger;
		$code->WeaponSet1->Sigil1 = ItemId::Superior_Sigil_of_Deamons2;
		$code->WeaponSet2->MainHand  = WeaponType::Staff;
		$code->WeaponSet2->Sigil2 = ItemId::Superior_Sigil_of_Concentration2;

		$effective = Statics::ResolveEffectiveWeapons($code, WeaponSetNumber::Set1);

		$this->assertEquals(WeaponType::Dagger, $effective->MainHand);
		$this->assertEquals(ItemId::Superior_Sigil_of_Deamons2, $effective->Sigil1);
		$this->assertEquals(WeaponType::_UNDEFINED, $effective->OffHand);
		$this->assertEquals(ItemId::_UNDEFINED, $effective->Sigil2);


		$reference = [ SkillId::Necrotic_Slash, SkillId::Life_Siphon, SkillId::Dark_Pact, SkillId::_UNDEFINED, SkillId::_UNDEFINED ];

		for($i = 0; $i < count($reference); $i++)
			$this->assertEquals($reference[$i], APICache::ResolveWeaponSkill($code, $effective, $i));
	}

	/** @test */
	public function ResolveTraitId()
	{
		$code = new BuildCode();
		$code->Profession = Profession::Mesmer;
		$code->Specializations->Choice1->SpecializationId = SpecializationId::Dueling;
		$code->Specializations->Choice1->Choices->Adept   = TraitLineChoice::MIDDLE;

		$id = APICache::ResolveTrait($code->Specializations->Choice1, TraitSlot::Adept);

		$this->assertEquals(TraitId::Desperate_Decoy, $id);
	}
}
