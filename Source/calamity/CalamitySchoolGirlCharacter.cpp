// Fill out your copyright notice in the Description page of Project Settings.

#include "CalamitySchoolGirlCharacter.h"


// Sets default values
ACalamitySchoolGirlCharacter::ACalamitySchoolGirlCharacter()
{
	// Set this character to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;
}

// Called when the game starts or when spawned
void ACalamitySchoolGirlCharacter::BeginPlay()
{
	Super::BeginPlay();
}

// Called every frame
void ACalamitySchoolGirlCharacter::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

}

// Called to bind functionality to input
void ACalamitySchoolGirlCharacter::SetupPlayerInputComponent(UInputComponent* PlayerInputComponent)
{
	Super::SetupPlayerInputComponent(PlayerInputComponent);

}

