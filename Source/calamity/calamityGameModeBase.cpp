// Fill out your copyright notice in the Description page of Project Settings.

#include "calamityGameModeBase.h"
#include "UObject/ConstructorHelpers.h"
#include "CalamitySchoolGirlCharacter.h"

AcalamityGameModeBase::AcalamityGameModeBase()
	: Super()
{
	// set default pawn class to our Blueprinted character
	static ConstructorHelpers::FClassFinder<APawn> PlayerPawnClassFinder(TEXT("/Game/Blueprints/Characters/B_SchoolGirl.B_SchoolGirl"));
	DefaultPawnClass = PlayerPawnClassFinder.Class;
}

