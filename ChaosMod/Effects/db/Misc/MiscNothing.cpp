#include <stdafx.h>

#include "Components/EffectDispatcher.h"

static const std::array options =
{
	"Nothing",
	"All Peds Are Peds",
	"Teleport To Current Location",
	"Expanded & Enhanced",
	"Spawn Air",
	"Destroy All Destroyed Vehicles",
	"Kill All Dead Peds",
	"+0 Wanted Stars",
	"Jesus Watches Over You",
	"Set Time To Current Time",
	"Set Player Into Current Vehicle",
	"All Cops Are Cops",
	"Aim to Point Gun",
	"Give Everyone A Nose",
	"Teleport Player A Few Millimeters",
	"Flying Birds",
	"Teleport All Elephants To Player",
	"PC Experience",
	"All Enemies Attack Player",
	"No Crash",
	" "
};

static void OnStart()
{
	const auto& effectOverride = options[g_Random.GetRandomInt(0, options.size() - 1)];
	GetComponent<EffectDispatcher>()->OverrideEffectName(EFFECT_NOTHING, effectOverride);
	WAIT(25000);
	
}
static RegisterEffect registerEffect(EFFECT_NOTHING, OnStart, EffectInfo
	{
		.Name = "Nothing",
		.Id = "nothing"
	}
);
