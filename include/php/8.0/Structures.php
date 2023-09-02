<?php namespace Hardstuck\GuildWars2\BuildCodes\V2;

use Hardstuck\GuildWars2\BuildCodes\V2\Util\AllEquipmentInfusions;
use Hardstuck\GuildWars2\BuildCodes\V2\Util\AllEquipmentStats;
use Hardstuck\GuildWars2\BuildCodes\V2\Util\AllSkills;
use Hardstuck\GuildWars2\BuildCodes\V2\Util\SpecializationChoices;
use Hardstuck\GuildWars2\BuildCodes\V2\Util\TraitLineChoices;

class BuildCode {
	public int                    $Version;
	public int                    $Kind;
	public int                    $Profession;
	public SpecializationChoices  $Specializations;
	public WeaponSet              $WeaponSet1;
	public WeaponSet              $WeaponSet2;
	public AllSkills              $SlotSkills;
	public int                    $Rune;
	public int                    $Relic;
	/** Note: For simplicity, pvp codes only have their amulet id set on the amulet. */
	public AllEquipmentStats      $EquipmentAttributes;
	public AllEquipmentInfusions  $Infusions;
	public int                    $Food;
	public int                    $Utility;
	public IProfessionSpecific    $ProfessionSpecific;
	public IArbitrary             $Arbitrary;

	public function __construct() {
		$this->Kind                = Kind::_UNDEFINED;
		$this->Profession          = Profession::_UNDEFINED;
		$this->Specializations     = new SpecializationChoices();
		$this->WeaponSet1          = new WeaponSet();
		$this->WeaponSet2          = new WeaponSet();
		$this->SlotSkills          = new AllSkills();
		$this->Rune                = ItemId::_UNDEFINED;
		$this->Relic               = ItemId::_UNDEFINED;
		$this->EquipmentAttributes = new AllEquipmentStats();
		$this->Infusions           = new AllEquipmentInfusions();
		$this->Food                = ItemId::_UNDEFINED;
		$this->Utility             = ItemId::_UNDEFINED;
		$this->ProfessionSpecific  = ProfessionSpecific\NONE::GetInstance();
		$this->Arbitrary           = Arbitrary\NONE::GetInstance();
	}
}

class Kind {
 	use Util\Enum;

	public const _UNDEFINED = 0;
	public const PvP        = 26 + 15;// ord('p') - ord('a');
	public const WvW        = 26 + 22;// ord('w') - ord('a');
	public const PvE        = 26 + 14;// ord('o') - ord('a');
}

//NOTE(Rennorb): names match official API
class Profession {
	use Util\Enum, Util\First;

	public const _UNDEFINED   = 0;
	public const Guardian     = 1;
	public const Warrior      = 2;
	public const Engineer     = 3;
	public const Ranger       = 4;
	public const Thief        = 5;
	public const Elementalist = 6;
	public const Mesmer       = 7;
	public const Necromancer  = 8;
	public const Revenant     = 9;
}

class WeightClass {
	use Util\Enum;

	public const _UNDEFINED = 0;
	public const Light      = 1;
	public const Medium     = 2;
	public const Heavy      = 3;
}

class Specialization {
	public int              $SpecializationId;
	public TraitLineChoices $Choices         ;

	public function __construct(int $specializationId = SpecializationId::_UNDEFINED, ?TraitLineChoices $choices = null) {
		$this->SpecializationId = $specializationId;
		$this->Choices          = $choices ?? new TraitLineChoices();
	}
}

//NOTE(Rennorb): this doesn't not have _UNDEFINED as to allow for usage in indexing TraitLineChoices[index]
class TraitSlot {
	public const Adept       = 0;
	public const Master      = 1;
	public const GrandMaster = 2;
}

class TraitLineChoice {
	use Util\Enum;

	public const NONE   = 0;
	public const TOP    = 1;
	public const MIDDLE = 2;
	public const BOTTOM = 3;
}

class WeaponSetNumber {
	use Util\Enum;

	public const _UNDEFINED = 0;
	public const Set1 = 1;
	public const Set2 = 2;
}

/** @remarks All fields might be _UNDEFINED. Twohanded weapons only set the MainHand, OffHand must be WeaponType::_UNDEFINED in that case. */
class WeaponSet {
	public function __construct(
		public int $MainHand = WeaponType::_UNDEFINED,
		public int $OffHand  = WeaponType::_UNDEFINED,
		public int $Sigil1   =     ItemId::_UNDEFINED,
		public int $Sigil2   =     ItemId::_UNDEFINED,
	) {}

	public function HasAny() : bool { return (bool)($this->MainHand | $this->OffHand); }
}

//NOTE(Rennorb): names match official API
class WeaponType {
	use Util\Enum;
	use Util\First;

	public const Nothing    =  0; // thief weapon mapping
	public const Axe        =  1;
	public const Dagger     =  2;
	public const Mace       =  3;
	public const Pistol     =  4;
	public const Sword      =  5;
	public const Scepter    =  6;
	public const Focus      =  7;
	public const Shield     =  8;
	public const Torch      =  9;
	public const Warhorn    = 10;

	public const Shortbow   = 11;
	public const Greatsword = 12;
	public const Hammer     = 13;
	public const Longbow    = 14;
	public const Rifle      = 15;
	public const Staff      = 16;
	public const HarpoonGun = 17;
	public const Spear      = 18;
	public const Trident    = 19;

	public const _UNDEFINED =  0;
}

interface IProfessionSpecific {}

namespace Hardstuck\GuildWars2\BuildCodes\V2\ProfessionSpecific;

class NONE implements \Hardstuck\GuildWars2\BuildCodes\V2\IProfessionSpecific {
	private static ?NONE $_instance = null;
	//TODO(Rennorb): replace these with calls to construct static
	public static function GetInstance() {
		if(NONE::$_instance === null) NONE::$_instance = new NONE();
		return NONE::$_instance;
	}
}

namespace Hardstuck\GuildWars2\BuildCodes\V2;


class RangerData implements IProfessionSpecific {
	/** @remarks Is PetId::_UNDEFINED if the pet is not set. */
	public int $Pet1 = PetId::_UNDEFINED;
	/** @remarks Is PetId::_UNDEFINED if the pet is not set. */
	public int $Pet2 = PetId::_UNDEFINED;
}

class PetId {
	use Util\Enum;

	public const _UNDEFINED = 0;
}

class RevenantData implements IProfessionSpecific {
	public int $Legend1 = Legend::SHIRO; // first
	/** @remarks Is Legend::_UNDEFINED if the Legend is not set. */
	public int $Legend2 = Legend::_UNDEFINED;

	/** @remarks Is SkillId::_UNDEFINED if the second Legend is not set. */
	public int $AltUtilitySkill1 = SkillId::_UNDEFINED;
	/** @remarks Is SkillId::_UNDEFINED if the second Legend is not set. */
	public int $AltUtilitySkill2 = SkillId::_UNDEFINED;
	/** @remarks Is SkillId::_UNDEFINED if the second Legend is not set. */
	public int $AltUtilitySkill3 = SkillId::_UNDEFINED;
}

class Legend {
	use Util\Enum;
	use Util\First;

	public const _UNDEFINED = 0;
	/** Assasin */
	public const SHIRO = 1;
	/** Dragon */
	public const GLINT = 2;
	/** Deamon */
	public const MALLYX = 3;
	/** Dwarf */
	public const JALIS = 4;
	/** Centaur */
	public const VENTARI = 5;
	/** Renegate */
	public const KALLA = 6;
	/** Alliance */
	public const VINDICATOR = 7;
}

interface IArbitrary { }

namespace Hardstuck\GuildWars2\BuildCodes\V2\Arbitrary;

class NONE implements \Hardstuck\GuildWars2\BuildCodes\V2\IArbitrary {
	private static ?NONE $_instance = null;
	//TODO(Rennorb): replace these with calls to construct static
	public static function GetInstance() {
		if(NONE::$_instance === null) NONE::$_instance = new NONE();
		return NONE::$_instance;
	}
}

namespace Hardstuck\GuildWars2\BuildCodes\V2;