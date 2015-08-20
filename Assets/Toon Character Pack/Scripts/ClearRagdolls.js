//Unfinished script to remove ragdolls when they reach a max amount (not optimized)
#pragma strict
var maxRagdolls:int;
var rag:GameObject;
function Start () {
	InvokeRepeating("ClearRags", 1,1);
}

function ClearRags () {

	
	var counter:int;
	for(var fooObj : GameObject in GameObject.FindGameObjectsWithTag("Player"))
		{		
    		if(fooObj.GetComponent.<Rigidbody>().useGravity==true){
    		if(!rag)
      		rag = fooObj;
      		counter++;
      		}
		}
	if(maxRagdolls < counter){
		Destroy(rag);
	}
}