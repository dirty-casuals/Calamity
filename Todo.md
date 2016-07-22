Things left
===========

+ Alter between rounds scene
  - Change "Next round you will be a..." to "Next Calamity you will become a..." and move it to the bottom so that the player/monster value also appears below the "Next..." text
+ End of game scene
  - Display lose and why to all if all characters die
  - Display lose and why to all if game reaches end and more than 1 character survive
  - Display lose and why to losers and win to sole winner if 1 character survives
+ Between state transitioning
  - In second round on Calamity event characters should be come toothy, if applicable
    * This almost works for AI and player but has an error bug
  - On second round end the characters should all return to normal and change to toothy, if applicable, on calamity event
+ Fix dead characters starting in the dead pose on round start
  - Probably just reset their anim state on end of round state start rather than pre calamity state start
+ Add white flash before blur on calamity event
+ Ensure that character toothy can kill characters
+ Add toothy swipe animation to animator
  - It's already in the animations in the project
+ Add the knife and make it work
  - Will need to add the stab animation (in project already) to the animator
  - Will probably need a limp animation for the characters
