using UnityEngine;
using UnityEditor;
using System;
using Google.GData.Client;

namespace GameDataEditor
{
	public class GDEOAuth
	{
		const string CLIENT_ID = "835206785031-e728g5seco0r583h6sivu0iota14ars4.apps.googleusercontent.com";
		const string CLIENT_SECRET = "WuxBy5qFjoy6XWVvlFTS4sdD";
		const string SCOPE = "https://www.googleapis.com/auth/drive";
		const string REDIRECT_URI = "urn:ietf:wg:oauth:2.0:oob";

		const int ACCESS_TOKEN_TIMEOUT = 3600;

		OAuth2Parameters oauth2Params;

		public string AccessToken
		{
			get { return oauth2Params.AccessToken; }
			private set {}
		}

        public GDEOAuth()
		{
			oauth2Params = new OAuth2Parameters();		

			oauth2Params.ClientId = CLIENT_ID;
			oauth2Params.ClientSecret = CLIENT_SECRET;
			oauth2Params.RedirectUri = REDIRECT_URI;
			oauth2Params.Scope = SCOPE;
		}

        public bool HasAuthenticated()
        {
			return !string.IsNullOrEmpty(GDESettings.Instance.AccessTokenKey);
        }

		public string GetAuthURL()
		{
			return OAuthUtil.CreateOAuth2AuthorizationUrl(oauth2Params);
		}

		public void SetAccessCode(string code)
		{
			if (oauth2Params != null)
			{
				oauth2Params.AccessCode = code;
				OAuthUtil.GetAccessToken(oauth2Params);
				SaveTokens();
			}
		}

		public void Init()
		{
			if (HasAuthenticated()) 
			{
				GDESettings settings = GDESettings.Instance;
				string accessToken = settings.AccessTokenKey;
				string refreshToken = settings.RefreshTokenKey;
				
				oauth2Params.AccessToken = accessToken;
				oauth2Params.RefreshToken = refreshToken;
				
				string timeString = settings.AccessTokenTimeout;
				DateTime lastRefreshed = DateTime.MinValue;
				
				if (!timeString.Equals (string.Empty))
					DateTime.Parse (timeString);
				
				TimeSpan timeSinceRefresh = DateTime.Now.Subtract (lastRefreshed);
				
				if (timeSinceRefresh.TotalSeconds >= ACCESS_TOKEN_TIMEOUT)
					RefreshAccessToken();
			}
		}

        public static void ClearAuth()
        {
			GDESettings settings = GDESettings.Instance;
			
			settings.AccessTokenTimeout = string.Empty;
			settings.AccessTokenKey = string.Empty;
			settings.RefreshTokenKey = string.Empty;
			
			settings.Save();

            Debug.Log(GDEConstants.ClearedAuthMsg);
        }

		void RefreshAccessToken()
		{
			OAuthUtil.RefreshAccessToken(oauth2Params);
			SaveTokens();
		}

		void SaveTokens()
		{
			GDESettings settings = GDESettings.Instance;
			
			settings.AccessTokenTimeout = DateTime.Now.ToString();
			settings.AccessTokenKey = oauth2Params.AccessToken;
			settings.RefreshTokenKey = oauth2Params.RefreshToken;
			
			settings.Save();
		}
	}
}


