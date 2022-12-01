<?php namespace Hardstuck\GuildWars2\BuildCodes\V2\Util;

use Hardstuck\GuildWars2\BuildCodes\V2\ItemId;
use Hardstuck\GuildWars2\BuildCodes\V2\SkillId;
use Hardstuck\GuildWars2\BuildCodes\V2\Specialization;
use Hardstuck\GuildWars2\BuildCodes\V2\StatId;
use Hardstuck\GuildWars2\BuildCodes\V2\TraitLineChoice;

use const Hardstuck\GuildWars2\BuildCodes\V2\ALL_INFUSION_COUNT;

class SpecializationChoices implements \ArrayAccess {
	public Specialization $Choice1;
	public Specialization $Choice2;
	public Specialization $Choice3;

	public function __construct() {
		$this->Choice1 = new Specialization();
		$this->Choice2 = new Specialization();
		$this->Choice3 = new Specialization();
	}

	/** @param int $offset */
	public function offsetGet(mixed $offset) : Specialization
	{
		return match ($offset) {
			0 => $this->Choice1,
			1 => $this->Choice2,
			2 => $this->Choice3,
			default => throw new \InvalidArgumentException("index($offset)"),
		};
	}

	/**
	 * @param int            $offset
	 * @param Specialization $value
	 */
	public function offsetSet(mixed $offset, mixed $value) : void 
	{
		switch($offset) {
			case 0: $this->Choice1 = $value; break;
			case 1: $this->Choice2 = $value; break;
			case 2: $this->Choice3 = $value; break;
			default: throw new \InvalidArgumentException("index($offset)");
		};
	}

	/** @param int $offset */
	public function offsetUnset(mixed $offset) : void 
	{
		switch($offset) {
			case 0: unset($this->Choice1); break;
			case 1: unset($this->Choice2); break;
			case 2: unset($this->Choice3); break;
			default: throw new \InvalidArgumentException("index($offset)");
		};
	}
	
	/** @param int $offset */
	public function offsetExists(mixed $offset) : bool { return 0 <= $offset && $offset <= 2; }
}

class TraitLineChoices implements \ArrayAccess {
	public int $Adept       = TraitLineChoice::NONE;
	public int $Master      = TraitLineChoice::NONE;
	public int $Grandmaster = TraitLineChoice::NONE;

	/** @param int $offset */
	public function offsetGet(mixed $offset) : int
	{
		return match ($offset) {
			0 => $this->Adept,
			1 => $this->Master,
			2 => $this->Grandmaster,
			default => throw new \InvalidArgumentException("index($offset)"),
		};
	}

	/**
	 * @param int $offset
	 * @param int $value
	 */
	public function offsetSet(mixed $offset, mixed $value) : void
	{
		switch($offset) {
			case 0: $this->Adept       = $value; break;
			case 1: $this->Master      = $value; break;
			case 2: $this->Grandmaster = $value; break;
			default: throw new \InvalidArgumentException("index($offset)");
		};
	}

	/** @param int $offset */
	public function offsetUnset(mixed $offset) : void 
	{
		switch ($offset) {
			case 0: unset($this->Adept);
			case 1: unset($this->Master);
			case 2: unset($this->Grandmaster);
			default: throw new \InvalidArgumentException("index($offset)");
		};
	}

	/** @param int $offset */
	public function offsetExists(mixed $offset) : bool { return 0 <= $offset && $offset <= 2; }
}

class AllSkills implements \ArrayAccess {
	public int $Heal     = SkillId::_UNDEFINED;
	public int $Utility1 = SkillId::_UNDEFINED;
	public int $Utility2 = SkillId::_UNDEFINED;
	public int $Utility3 = SkillId::_UNDEFINED;
	public int $Elite    = SkillId::_UNDEFINED;

	/** @param int $offset */
	public function offsetGet(mixed $offset) : int
	{
		return match ($offset) {
			0 => $this->Heal,
			1 => $this->Utility1,
			2 => $this->Utility2,
			3 => $this->Utility3,
			4 => $this->Elite,
			default => throw new \InvalidArgumentException("index($offset)"),
		};
	}

	/**
	 * @param int $offset
	 * @param int $value
	 */
	public function offsetSet(mixed $offset, mixed $value) : void
	{
		switch($offset) {
			case 0: $this->Heal     = $value; break;
			case 1: $this->Utility1 = $value; break;
			case 2: $this->Utility2 = $value; break;
			case 3: $this->Utility3 = $value; break;
			case 4: $this->Elite    = $value; break;
			default: throw new \InvalidArgumentException("index($offset)");
		};
	}

		/** @param int $offset */
	public function offsetUnset(mixed $offset) : void 
	{
		switch ($offset) {
			case 0: unset($this->Heal);
			case 1: unset($this->Utility1);
			case 2: unset($this->Utility2);
			case 3: unset($this->Utility3);
			case 4: unset($this->Elite);
			default: throw new \InvalidArgumentException("index($offset)");
		};
	}

	/** @param int $offset */
	public function offsetExists(mixed $offset) : bool { return 0 <= $offset && $offset <= 4; }
}

class AllEquipmentStats implements \ArrayAccess {
	public int $Helmet             = StatId::_UNDEFINED;
	public int $Shoulders          = StatId::_UNDEFINED;
	public int $Chest              = StatId::_UNDEFINED;
	public int $Gloves             = StatId::_UNDEFINED;
	public int $Leggings           = StatId::_UNDEFINED;
	public int $Boots              = StatId::_UNDEFINED;
	public int $BackItem           = StatId::_UNDEFINED;
	public int $Accessory1         = StatId::_UNDEFINED;
	public int $Accessory2         = StatId::_UNDEFINED;
	public int $Ring1              = StatId::_UNDEFINED;
	public int $Ring2              = StatId::_UNDEFINED;
	/** @remarks Is StatId::_UNDEFINED if the weapon is not set. */
	public int $WeaponSet1MainHand = StatId::_UNDEFINED;
	/** @remarks Is StatId::_UNDEFINED if the weapon is not set. */
	public int $WeaponSet1OffHand  = StatId::_UNDEFINED;
	/** @remarks Is StatId::_UNDEFINED if the weapon is not set. */
	public int $WeaponSet2MainHand = StatId::_UNDEFINED;
	/** @remarks Is StatId::_UNDEFINED if the weapon is not set. */
	public int $WeaponSet2OffHand  = StatId::_UNDEFINED;
	public int $Amulet             = StatId::_UNDEFINED;

	/** @param int $offset */
	public function offsetGet(mixed $offset) : int
	{
		return match ($offset) {
			 0 => $this->Helmet,
			 1 => $this->Shoulders,
			 2 => $this->Chest,
			 3 => $this->Gloves,
			 4 => $this->Leggings,
			 5 => $this->Boots,
			 6 => $this->BackItem,
			 7 => $this->Accessory1,
			 8 => $this->Accessory2,
			 9 => $this->Ring1,
			10 => $this->Ring2,
			11 => $this->WeaponSet1MainHand,
			12 => $this->WeaponSet1OffHand ,
			13 => $this->WeaponSet2MainHand,
			14 => $this->WeaponSet2OffHand ,
			15 => $this->Amulet,
			default => throw new \InvalidArgumentException("index($offset)"),
		};
	}
	
	/**
	 * @param int $offset
	 * @param int $value
	 */
	public function offsetSet(mixed $offset, mixed $value) : void
	{
		switch($offset) {
			case  0: $this->Helmet             = $value; break;
			case  1: $this->Shoulders          = $value; break;
			case  2: $this->Chest              = $value; break;
			case  3: $this->Gloves             = $value; break;
			case  4: $this->Leggings           = $value; break;
			case  5: $this->Boots              = $value; break;
			case  6: $this->BackItem           = $value; break;
			case  7: $this->Accessory1         = $value; break;
			case  8: $this->Accessory2         = $value; break;
			case  9: $this->Ring1              = $value; break;
			case 10: $this->Ring2              = $value; break;
			case 11: $this->WeaponSet1MainHand = $value; break;
			case 12: $this->WeaponSet1OffHand  = $value; break;
			case 13: $this->WeaponSet2MainHand = $value; break;
			case 14: $this->WeaponSet2OffHand  = $value; break;
			case 15: $this->Amulet             = $value; break;
			default: throw new \InvalidArgumentException("index($offset)");
		};
	}

		/** @param int $offset */
	public function offsetUnset(mixed $offset) : void 
	{
		switch ($offset) {
			case  0: unset($this->Helmet);
			case  1: unset($this->Shoulders);
			case  2: unset($this->Chest);
			case  3: unset($this->Gloves);
			case  4: unset($this->Leggings);
			case  5: unset($this->Boots);
			case  6: unset($this->BackItem);
			case  7: unset($this->Accessory1);
			case  8: unset($this->Accessory2);
			case  9: unset($this->Ring1);
			case 10: unset($this->Ring2);
			case 11: unset($this->WeaponSet1MainHand);
			case 12: unset($this->WeaponSet1OffHand );
			case 13: unset($this->WeaponSet2MainHand);
			case 14: unset($this->WeaponSet2OffHand );
			case 15: unset($this->Amulet);
			default: throw new \InvalidArgumentException("index($offset)");
		};
	}

	/** @param int $offset */
	public function offsetExists(mixed $offset) : bool { return 0 <= $offset && $offset <= 15; }
}

class AllEquipmentInfusions implements \ArrayAccess {
	public int $Helmet       = ItemId::_UNDEFINED;
	public int $Shoulders    = ItemId::_UNDEFINED;
	public int $Chest        = ItemId::_UNDEFINED;
	public int $Gloves       = ItemId::_UNDEFINED;
	public int $Leggings     = ItemId::_UNDEFINED;
	public int $Boots        = ItemId::_UNDEFINED;
	public int $BackItem_1   = ItemId::_UNDEFINED;
	public int $BackItem_2   = ItemId::_UNDEFINED;
	public int $Accessory1   = ItemId::_UNDEFINED;
	public int $Accessory2   = ItemId::_UNDEFINED;
	public int $Ring1_1      = ItemId::_UNDEFINED;
	public int $Ring1_2      = ItemId::_UNDEFINED;
	public int $Ring1_3      = ItemId::_UNDEFINED;
	public int $Ring2_1      = ItemId::_UNDEFINED;
	public int $Ring2_2      = ItemId::_UNDEFINED;
	public int $Ring2_3      = ItemId::_UNDEFINED;
	public int $WeaponSet1_1 = ItemId::_UNDEFINED;
	public int $WeaponSet1_2 = ItemId::_UNDEFINED;
	public int $WeaponSet2_1 = ItemId::_UNDEFINED;
	public int $WeaponSet2_2 = ItemId::_UNDEFINED;
	public int $Amulet       = ItemId::_UNDEFINED;

	/** @param int $offset */
	public function offsetGet(mixed $offset) : int
	{
		return match ($offset) {
			 0 => $this->Helmet,
			 1 => $this->Shoulders,
			 2 => $this->Chest,
			 3 => $this->Gloves,
			 4 => $this->Leggings,
			 5 => $this->Boots,
			 6 => $this->BackItem_1,
			 7 => $this->BackItem_2,
			 8 => $this->Accessory1,
			 9 => $this->Accessory2,
			10 => $this->Ring1_1,
			11 => $this->Ring1_2,
			12 => $this->Ring1_3,
			13 => $this->Ring2_1,
			14 => $this->Ring2_2,
			15 => $this->Ring2_3,
			16 => $this->WeaponSet1_1,
			17 => $this->WeaponSet1_2,
			18 => $this->WeaponSet2_1,
			19 => $this->WeaponSet2_2,
			20 => $this->Amulet,
			default => throw new \InvalidArgumentException("index($offset)"),
		};
	}

	/**
	 * @param int $offset
	 * @param int $value
	 */
	public function offsetSet(mixed $offset, mixed $value) : void
	{
		switch($offset) {
			case  0: $this->Helmet       = $value; break;
			case  1: $this->Shoulders    = $value; break;
			case  2: $this->Chest        = $value; break;
			case  3: $this->Gloves       = $value; break;
			case  4: $this->Leggings     = $value; break;
			case  5: $this->Boots        = $value; break;
			case  6: $this->BackItem_1   = $value; break;
			case  7: $this->BackItem_2   = $value; break;
			case  8: $this->Accessory1   = $value; break;
			case  9: $this->Accessory2   = $value; break;
			case 10: $this->Ring1_1      = $value; break;
			case 11: $this->Ring1_2      = $value; break;
			case 12: $this->Ring1_3      = $value; break;
			case 13: $this->Ring2_1      = $value; break;
			case 14: $this->Ring2_2      = $value; break;
			case 15: $this->Ring2_3      = $value; break;
			case 16: $this->WeaponSet1_1 = $value; break;
			case 17: $this->WeaponSet1_2 = $value; break;
			case 18: $this->WeaponSet2_1 = $value; break;
			case 19: $this->WeaponSet2_2 = $value; break;
			case 20: $this->Amulet       = $value; break;
			default: throw new \InvalidArgumentException("index($offset)");
		};
	}

		/** @param int $offset */
	public function offsetUnset(mixed $offset) : void 
	{
		switch ($offset) {
			case  0: unset($this->Helmet);
			case  1: unset($this->Shoulders);
			case  2: unset($this->Chest);
			case  3: unset($this->Gloves);
			case  4: unset($this->Leggings);
			case  5: unset($this->Boots);
			case  6: unset($this->BackItem_1);
			case  7: unset($this->BackItem_2);
			case  8: unset($this->Accessory1);
			case  9: unset($this->Accessory2);
			case 10: unset($this->Ring1_1);
			case 11: unset($this->Ring1_2);
			case 12: unset($this->Ring1_3);
			case 13: unset($this->Ring2_1);
			case 14: unset($this->Ring2_2);
			case 15: unset($this->Ring2_3);
			case 16: unset($this->WeaponSet1_1);
			case 17: unset($this->WeaponSet1_2);
			case 18: unset($this->WeaponSet2_1);
			case 19: unset($this->WeaponSet2_2);
			case 20: unset($this->Amulet);
			default: throw new \InvalidArgumentException("index($offset)");
		};
	}

	/** @param int $offset */
	public function offsetExists(mixed $offset) : bool { return 0 <= $offset && $offset <= 20; }

	//NOTE(Rennorb): It isn't really optimal to use this performance wise, but its very convenient.
	public function HasAny() : bool
	{
		for($i = 0; $i < ALL_INFUSION_COUNT; $i++)
			if($this[$i] !== ItemId::_UNDEFINED)
				return true;
		return false;
	}
}
