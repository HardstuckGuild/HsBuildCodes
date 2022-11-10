<?php namespace Hardstuck\GuildWars2\BuildCodes\V2\Tests\API;

use Hardstuck\GuildWars2\BuildCodes\V2\Statics;
use PHPUnit\Framework\TestCase;

class VersionTests extends TestCase {
	public function DataProvider() {
		$invalidCodes = [
			'x____________________________',
		];
		
		$v1Codes = [
			'v0_-------------------------',
		];
		
		$v2Codes = [
			'c_____________',
			'C_____________',
		];

		return array_merge(
			array_map(fn($code) => [$code, -1], $invalidCodes),
			array_map(fn($code) => [$code, 1], $v1Codes),
			array_map(fn($code) => [$code, 2], $v2Codes),
		 );
	}

	/** @test @dataProvider DataProvider */
	public function VersionDetectionTest(string $code, int $expectedVersion) {
		$this->assertEquals($expectedVersion, Statics::DetermineCodeVersion($code));
	}
}
