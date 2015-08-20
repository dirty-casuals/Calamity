#pragma strict
import System.Collections.Generic;	//Used to sort list
var _animator:Animator;
var _animations:String[];
var _crossFade:boolean;
var maxButtons:int = 10;			//Maximum buttons per page	

var removeTextFromButton:String;	//Unwanted text 
var autoChangeDelay:float;
var image:GUITexture;
var _lastAnim:String;
private var page:int = 0;			//Current page
private var pages:int;				//Number of pages

private var _active:boolean = true;
private var counter:int = -1;


function Start(){
	//Sort list alphabeticly
    _animations.Sort(_animations, function(g1,g2) String.Compare(g1, g2));
	pages = Mathf.Ceil((_animations.length -1 )/ maxButtons);	
}

function OnGUI () {
	if(!_animator)
	_animator = Transform.FindObjectOfType(Animator);
	if(_active){
	//Time Scale Vertical Slider
	//Time.timeScale = GUI.VerticalSlider (Rect (185, 50, 20, 150), Time.timeScale, 2.0, 0.0);
	//Check if there are more in list than max buttons (true adds "next" and "prev" buttons)
	if(_animations.length > maxButtons){
		//Prev button
		if(GUI.Button(Rect(20,(maxButtons+1)*18,75,18),"Prev"))if(page > 0)page--;else page=pages;
		//Next button
		if(GUI.Button(Rect(95,(maxButtons+1)*18,75,18),"Next"))if(page < pages)page++;else page=0;
		//Page text
		GUI.Label (Rect(60,(maxButtons+2)*18,150,22), "Page" + (page+1) + " / " + (pages+1));
	}
	//Calculate how many buttons on current page (last page might have less)
	var pageButtonCount:int = _animations.length - (page*maxButtons);
	//Debug.Log(pageButtonCount);
	if(pageButtonCount > maxButtons)pageButtonCount = maxButtons;
	//Adds buttons based on how many particle systems on page
	for(var i:int=0;i < pageButtonCount;i++){
		var buttonText:String = _animations[i+(page*maxButtons)];
		if(removeTextFromButton != "")
		buttonText = buttonText.Replace(removeTextFromButton, "");
		if(GUI.Button(Rect(20,i*18+18,150,18),buttonText)){
			if(_crossFade){	
			if(_lastAnim == (_animations[i+page*maxButtons]))
			this._animator.Play("");
			_animator.CrossFade(_animations[i+page*maxButtons], .1);
			this._lastAnim = _animations[i+page*maxButtons];
			}else{
			
			_animator.Play(_animations[i+page*maxButtons]);
			}
			counter = i + (page * maxButtons);	
		}
	}
	}
	if(image){
			image.pixelInset.x =  (Screen.width) -(image.texture.width) ;
		}
}