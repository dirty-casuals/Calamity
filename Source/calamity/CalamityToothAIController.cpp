// Fill out your copyright notice in the Description page of Project Settings.

#include "CalamityToothAIController.h"
#include "CalamityToothyCharacter.h"


ACalamityToothAIController::ACalamityToothAIController(const class FObjectInitializer& ObjectInitializer)
	: Super(ObjectInitializer)
{
	BehaviorComp = ObjectInitializer.CreateDefaultSubobject<UBehaviorTreeComponent>(this, TEXT("BehaviorComp"));
	BlackboardComp = ObjectInitializer.CreateDefaultSubobject<UBlackboardComponent>(this, TEXT("BlackboardComp"));
	CurrentWaypointKeyName = "";
}

void ACalamityToothAIController::Possess(class APawn* InPawn)
{
	Super::Possess(InPawn);

	ACalamityToothyCharacter* Toothy = Cast<ACalamityToothyCharacter>(InPawn);
	if (Toothy)
	{
		if (Toothy->BehaviorTree->BlackboardAsset)
		{
			BlackboardComp->InitializeBlackboard(*Toothy->BehaviorTree->BlackboardAsset);
		}

		BehaviorComp->StartTree(*Toothy->BehaviorTree);
	}
}


void ACalamityToothAIController::UnPossess()
{
	Super::UnPossess();

	/* Stop any behavior running as we no longer have a pawn to control */
	BehaviorComp->StopTree();
}


void ACalamityToothAIController::SetWaypoint(ACalamityWayPoint* NewWaypoint)
{
	if (BlackboardComp)
	{
		BlackboardComp->SetValueAsObject(CurrentWaypointKeyName, NewWaypoint);
	}
}


ACalamityWayPoint* ACalamityToothAIController::GetWaypoint()
{
	if (BlackboardComp)
	{
		return Cast<ACalamityWayPoint>(BlackboardComp->GetValueAsObject(CurrentWaypointKeyName));
	}

	return nullptr;
}
