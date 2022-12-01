<?php namespace Hardstuck\GuildWars2\BuildCodes\V2;

set_include_path(get_include_path() . PATH_SEPARATOR . __DIR__.'/../../common/');
//NOTE(Rennorb): Include the statics directly since the autoloader will only be called for classes.
require_once __DIR__.'/Database/Static.php';

spl_autoload_register(function($class) {
	$relNamespace = strstr($class, __NAMESPACE__);
	if($relNamespace === false) return;
	$relNamespace = substr($class, strlen(__NAMESPACE__));
	//print "\n$class - ".__NAMESPACE__." -> [[$relNamespace]]\n";
	switch($relNamespace) {
		case '\Util\StringView':
		case '\APILoader':
		case '\TextLoader':
		case '\BinaryLoader':
			require __DIR__.str_replace('\\', '/', $relNamespace).'.php';
			break;

		case '\Overrides':
		case '\PerProfessionData':
		case '\LazyLoadMode':
			require __DIR__.'/Database'.str_replace('\\', '/', $relNamespace).'.php';
			break;
		
		case '\ItemId':
		case '\SkillId':
		case '\SpecializationId':
		case '\StatId':
		case '\TraitId':
			require __DIR__.'/Database'.str_replace('\\', '/', $relNamespace).'s.php';
			break;

		case '\ICache':
		case '\DefaultCacheImpl':
			require_once __DIR__.'/OfficialAPI/CacheImpl.php';
			break;

		case '\API':
		case '\APICache':
			require __DIR__.'/OfficialAPI'.str_replace('\\', '/', $relNamespace).'.php';
			break;

		case '\Util\FromName':
		case '\Util\First':
		case '\Util\Enum':
		case '\Util\_Static':
			require_once __DIR__.'/Util/Statics.php';
			break;

		case '\Util\SpecializationChoices':
		case '\Util\TraitLineChoices':
		case '\Util\AllSkills':
		case '\Util\AllEquipmentStats':
		case '\Util\AllEquipmentInfusions':
			require_once __DIR__.'/Util/UtilStructs.php';
			break;

		case '\BuildCode':
		case '\Kind':
		case '\Profession':
		case '\Specialization':
		case '\TraitSlot':
		case '\TraitLineChoice':
		case '\WeaponSetNumber':
		case '\WeaponSet':
		case '\WeaponType':
		case '\WeightClass':
		case '\IProfessionSpecific':
		case '\ProfessionSpecific\NONE':
		case '\RangerData':
		case '\PetId':
		case '\RevenantData':
		case '\Legend':
		case '\IArbitrary':
		case '\Arbitrary\NONE':
			require_once __DIR__.'/Structures.php';
			break;

		case '\BitReader':
		case '\BitWriter':
			require_once __DIR__.'/BinaryLoader.php';
			break;
	}

	if(method_exists($class, '__construct_static')) {
		//print "\n static constructing $class\n";
		$class::__construct_static();
	}
});