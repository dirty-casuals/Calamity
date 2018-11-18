// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "CalamitySchoolGirlCharacter.h"
#include "CalamitySchoolGirlAI.generated.h"

/**
 *
 */
UCLASS()
class CALAMITY_API ACalamitySchoolGirlAI : public ACalamitySchoolGirlCharacter
{
	GENERATED_BODY()

public:
	// Sets default values for this character's properties
	ACalamitySchoolGirlAI();

protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;

public:
	// Called every frame
	virtual void Tick(float DeltaTime) override;





};
