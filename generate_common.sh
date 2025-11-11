#!/bin/bash
set -eo pipefail

API_ROOT="https://api.guildwars2.com/v2"
COMMON_OFFLINE_ROOT=include/common/offline

cd "$(dirname "$0")" # move to script location

for spec in Elementalist Engineer Guardian Mesmer Necromancer Ranger Revenant Thief Warrior; do
	echo Working on $spec

	tmpfile="${TMPDIR:-/tmp/}$spec.json"
	curl -sH "X-Schema-Version: 2019-12-19T00:00:00.000Z" "$API_ROOT/professions/$spec" > $tmpfile

	spec_file="$COMMON_OFFLINE_ROOT/specializations/$spec.csv"
	printf "Specialization Index;SpecializationId\n" > $spec_file
	jq -cr '.specializations| to_entries | .[] | "\(.key);\(.value)"' $tmpfile >> $spec_file

	palette_file="$COMMON_OFFLINE_ROOT/pallettes/$spec.csv"
	printf "PalletteId;SkillId\n" > $palette_file
	jq -cr '.skills_by_palette[] | "\(.[0]);\(.[1])"' $tmpfile >> $palette_file
done
