<?php namespace Hardstuck\GuildWars2\BuildCodes\V2\Tests\Database;

use Hardstuck\GuildWars2\BuildCodes\V2\PerProfessionData;
use Hardstuck\GuildWars2\BuildCodes\V2\Profession;
use PHPUnit\Framework\TestCase;

set_include_path(__DIR__.'/../../../include/common/');

final class DatabaseInteractionTests extends TestCase
{
	/** @test */
	public function CanDownloadSkillPallettes() : void
	{
		PerProfessionData::Reload(Profession::Revenant);
		$this->assertGreaterThan(2, count(PerProfessionData::$Revenant->SkillToPallette));
	}

	/** @test */
	public function CanFindOfflinePallette() : void
	{
		PerProfessionData::Reload(Profession::Necromancer, true);
		$this->assertGreaterThan(2, count(PerProfessionData::$Necromancer->SkillToPallette));
	}
}