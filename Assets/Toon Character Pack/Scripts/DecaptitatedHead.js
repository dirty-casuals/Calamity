/****************************************
	DecapitatedHead.js
	Adds some force to the head and disables the rigidbodies kinematic
	
	Copyright 2013 Unluck Software	
 	www.chemicalbliss.com											
*****************************************/
#pragma strict
var force:Vector3;
var delay:float = 0.25;
function Start () {
	yield(WaitForSeconds(delay));
	GetComponent.<Rigidbody>().isKinematic = false;
	GetComponent.<Rigidbody>().AddForce(force*Random.value);
	GetComponent.<Rigidbody>().AddTorque(Vector3(Random.Range(-1, 1),Random.Range(-1, 1),Random.Range(-1, 1)));
}