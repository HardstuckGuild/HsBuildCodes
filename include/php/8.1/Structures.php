<?php namespace Hardstuck\GuildWars2\BuildCodes\V2;

use Hardstuck\GuildWars2\BuildCodes\V2\Util\AllEquipmentInfusions;
use Hardstuck\GuildWars2\BuildCodes\V2\Util\AllEquipmentStats;
use Hardstuck\GuildWars2\BuildCodes\V2\Util\AllSkills;
use Hardstuck\GuildWars2\BuildCodes\V2\Util\SpecializationChoices;
use Hardstuck\GuildWars2\BuildCodes\V2\Util\TraitLineChoices;

class BuildCode {
	public int                    $Version;
	public Kind                   $Kind;
	public Profession             $Profession;
	public SpecializationChoices  $Specializations;
	public WeaponSet              $WeaponSet1;
	public WeaponSet              $WeaponSet2;
	public AllSkills              $SlotSkills;
	public int                    $Rune;
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
		$this->EquipmentAttributes = new AllEquipmentStats();
		$this->Infusions           = new AllEquipmentInfusions();
		$this->Food                = ItemId::_UNDEFINED;
		$this->Utility             = ItemId::_UNDEFINED;
		$this->ProfessionSpecific  = ProfessionSpecific\NONE::GetInstance();
		$this->Arbitrary           = Arbitrary\NONE::GetInstance();
	}
}

enum Kind : int {
	case _UNDEFINED = 0;
	case PvP        = 26 + 15;// ord('p') - ord('a');
	case WvW        = 26 + 22;// ord('w') - ord('a');
	case PvE        = 26 + 14;// ord('o') - ord('a');
}

//NOTE(Rennorb): names match official API
enum Profession : int {
	use Util\FromName;

	case _UNDEFINED   = 0;
	case Guardian     = 1;
	case Warrior      = 2;
	case Engineer     = 3;
	case Ranger       = 4;
	case Thief        = 5;
	case Elementalist = 6;
	case Mesmer       = 7;
	case Necromancer  = 8;
	case Revenant     = 9;
}

class Specialization {
	public function __construct(
		public int              $SpecializationId = SpecializationId::_UNDEFINED,
		public TraitLineChoices $Choices          = new TraitLineChoices(),
	) {}
}

enum TraitSlot : int {
	case Adept       = 0;
	case Master      = 1;
	case GrandMaster = 2;
}

enum TraitLineChoice : int {
	case NONE   = 0;
	case TOP    = 1;
	case MIDDLE = 2;
	case BOTTOM = 3;
}

enum WeaponSetNumber : int {
	case _UNDEFINED = 0;
	case Set1 = 1;
	case Set2 = 2;
}

/** @remarks All fields might be _UNDEFINED. Twohanded weapons only set the MainHand, OffHand must be WeaponType::_UNDEFINED in that case. */
class WeaponSet {
	public function __construct(
		public WeaponType $MainHand = WeaponType::_UNDEFINED,
		public WeaponType $OffHand  = WeaponType::_UNDEFINED,
		public int        $Sigil1   =     ItemId::_UNDEFINED,
		public int        $Sigil2   =     ItemId::_UNDEFINED,
	) {}

	public function HasAny() : bool { return (bool)($this->MainHand->value & $this->OffHand->value); }
}

//NOTE(Rennorb): names match official API
enum WeaponType : int {
	use Util\FromName;
	use Util\First;

	case _UNDEFINED =  0;
	case Axe        =  1;
	case Dagger     =  2;
	case Mace       =  3;
	case Pistol     =  4;
	case Sword      =  5;
	case Scepter    =  6;
	case Focus      =  7;
	case Shield     =  8;
	case Torch      =  9;
	case Warhorn    = 10;
	case ShortBow   = 11;

	case Greatsword = 12;
	case Hammer     = 13;
	case Longbow    = 14;
	case Rifle      = 15;
	case Staff      = 16;
	case HarpoonGun = 17;
	case Spear      = 18;
	case Trident    = 19;
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
	public int $Legend1 = Legend::_UNDEFINED;
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