#include <stdafx.h>

#include "Effects/MetaModifiers.h"

#include "Components/EffectDispatcher.h"

static void OnStart()
{
	ClearEntityPool();

	GetComponent<EffectDispatcher>()->ClearActiveEffects(EFFECT_META_NO_CHAOS);
}

static void OnStop()
{
	MetaModifiers::m_bDisableChaos = false;
}

static void OnTick()
{
	MetaModifiers::m_bDisableChaos = true;
}

static RegisterEffect registerEffect(EFFECT_META_NO_CHAOS, OnStart, OnStop, OnTick, EffectInfo
	{
		.Name = "No Chaos",
		.Id = "meta_nochaos",
		.IsTimed = true,
		.IncompatibleWith = { EFFECT_META_HIDE_CHAOS_UI },
		.ExecutionType = EEffectExecutionType::Meta
	}
);