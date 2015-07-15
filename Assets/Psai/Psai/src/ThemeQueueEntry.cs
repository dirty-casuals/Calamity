//-----------------------------------------------------------------------
// <copyright company="Periscope Studio">
//     Copyright (c) Periscope Studio UG & Co. KG. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Text;

namespace psai.net
{
	// struct used for enqueueing Theme changes, that will be executed when the intensity has dropped down to zero
	internal class ThemeQueueEntry : System.ICloneable
	{
		internal PsaiPlayMode playmode;
		internal int themeId;
		internal float startIntensity;		
		internal int restTimeMillis;		// if !=0, the theme will be started in restMode for x milliseconds
		internal bool holdIntensity;
        internal int musicDuration;

		internal ThemeQueueEntry()
		{
			playmode = PsaiPlayMode.regular;
			themeId = -1;
			startIntensity = 1.0f;
			restTimeMillis = 0;
			holdIntensity = false;
		}

        public System.Object Clone()
        {
            return (ThemeQueueEntry)this.MemberwiseClone();
        }
     
	};
}
