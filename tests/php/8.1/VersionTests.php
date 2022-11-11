<?php namespace Hardstuck\GuildWars2\BuildCodes\V2\Tests\API;

require_once 'TestUtilities.php';

use Hardstuck\GuildWars2\BuildCodes\V2\Statics;
use Hardstuck\GuildWars2\BuildCodes\V2\Tests\TestUtilities;
use Hardstuck\GuildWars2\BuildCodes\V2\TextLoader;
use PHPUnit\Framework\TestCase;

class VersionTests extends TestCase {
	public function DataProvider() {
		$invalidCodes = [
			TestUtilities::$CodesInvalid["wrong-version"],
		];
		
		$v1Codes = TestUtilities::$CodesV1;
		
		$v2Codes = TestUtilities::$CodesV2;

		return array_merge(
			array_map(fn($code) => [$code, -1], $invalidCodes),
			array_map(fn($code) => [$code, 1], $v1Codes),
			array_map(function($code) {
				$version = TextLoader::INVERSE_CHARSET[ord($code[0])];
				if($version >= 26) $version -= 26;
				return [$code, $version];
			}, $v2Codes),
		 );
	}

	/** @test @dataProvider DataProvider */
	public function VersionDetectionTest(string $code, int $expectedVersion) {
		$this->assertEquals($expectedVersion, Statics::DetermineCodeVersion($code));
	}
}
