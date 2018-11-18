// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "AIController.h"
#include "CalamityWayPoint.h"
#include "BehaviorTree/BlackboardComponent.h"
#include "CalamityToothAIController.generated.h"

class UBehaviorTreeComponent;

/**
 *
 */
UCLASS()
class CALAMITY_API ACalamityToothAIController : public AAIController
{
	GENERATED_BODY()

		ACalamityToothAIController(const class FObjectInitializer& ObjectInitializer);

	virtual void Possess(class APawn* InPawn) override;

	virtual void UnPossess() override;

	UBehaviorTreeComponent* BehaviorComp;

	UBlackboardComponent* BlackboardComp;

	UPROPERTY(EditDefaultsOnly, Category = "AI")
		FName CurrentWaypointKeyName;
public:

	ACalamityWayPoint* GetWaypoint();

	void SetWaypoint(ACalamityWayPoint* NewWaypoint);

	FORCEINLINE UBehaviorTreeComponent* GetBehaviorComp() const { return BehaviorComp; }

	FORCEINLINE UBlackboardComponent* GetBlackboardComp() const { return BlackboardComp; }

};
