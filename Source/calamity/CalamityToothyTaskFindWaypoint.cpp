// Fill out your copyright notice in the Description page of Project Settings.

#include "CalamityToothyTaskFindWaypoint.h"
#include "calamity.h"
#include "CalamityToothAIController.h"
#include "CalamityWayPoint.h"
#include "Kismet/GameplayStatics.h"



#include "BehaviorTree/BehaviorTreeComponent.h"
#include "BehaviorTree/BlackboardComponent.h"
#include "BehaviorTree/Blackboard/BlackboardKeyAllTypes.h"


EBTNodeResult::Type UCalamityToothyTaskFindWaypoint::ExecuteTask(UBehaviorTreeComponent& OwnerComp, uint8* NodeMemory)
{
	ACalamityToothAIController* ToothyController = Cast<ACalamityToothAIController>(OwnerComp.GetAIOwner());
	if (ToothyController == nullptr)
	{
		return EBTNodeResult::Failed;
	}

	ACalamityWayPoint* CurrentWaypoint = ToothyController->GetWaypoint();
	AActor* NewWaypoint = nullptr;

	/* Iterate all the bot waypoints in the current level and find a new random waypoint to set as destination */
	TArray<AActor*> AllWaypoints;
	UGameplayStatics::GetAllActorsOfClass(ToothyController, ACalamityWayPoint::StaticClass(), AllWaypoints);

	if (AllWaypoints.Num() == 0)
		return EBTNodeResult::Failed;

	/* Find a new waypoint randomly by index (this can include the current waypoint) */
	/* For more complex or human AI you could add some weights based on distance and other environmental conditions here */
	NewWaypoint = AllWaypoints[FMath::RandRange(0, AllWaypoints.Num() - 1)];

	/* Assign the new waypoint to the Blackboard */
	if (NewWaypoint)
	{
		/* The selected key should be "CurrentWaypoint" in the BehaviorTree setup */
		OwnerComp.GetBlackboardComponent()->SetValue<UBlackboardKeyType_Object>(BlackboardKey.GetSelectedKeyID(), NewWaypoint);
		return EBTNodeResult::Succeeded;
	}

	return EBTNodeResult::Failed;
}


