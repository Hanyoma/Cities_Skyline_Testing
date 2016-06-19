﻿using ICities;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.Plugins;
using System.Reflection;
using System;
using System.Collections;
using NetworkAPI;

//many parts taken from:
//https://github.com/AlexanderDzhoganov/Skylines-DynamicResolution/

namespace NetworkAPI
{
	public class NetworkAPIMod : IUserMod
	{
		public string Name { get { return "Network API"; } }
		public string Description { get { return "This mod exposes the Cities: Skylines Data and Interfaces Through Sockets."; } }
	}

	public class LoadingExtension : LoadingExtensionBase
	{

		public override void OnLevelLoaded(LoadMode mode)
		{
			if (mode != LoadMode.NewGame && mode != LoadMode.LoadGame)
			{
				return;
			}

			// this seems to get the default UIView
			UIView uiView = UIView.GetAView ();

            // example for adding a button

			// Add a new button to the view.
			var button = (UIButton)uiView.AddUIComponent(typeof(UIButton));

			// Set the text to show on the button.
			button.text = "Start Server";

			// Set the button dimensions.
			button.width = 200;
			button.height = 30;

			// Style the button to look like a menu button.
			button.normalBgSprite = "ButtonMenu";
			button.disabledBgSprite = "ButtonMenuDisabled";
			button.hoveredBgSprite = "ButtonMenuHovered";
			button.focusedBgSprite = "ButtonMenuFocused";
			button.pressedBgSprite = "ButtonMenuPressed";
			button.textColor = new Color32(255, 255, 255, 255);
			button.disabledTextColor = new Color32(7, 7, 7, 255);
			button.hoveredTextColor = new Color32(7, 132, 255, 255);
			button.focusedTextColor = new Color32(255, 255, 255, 255);
			button.pressedTextColor = new Color32(30, 30, 44, 255);

			// Enable button sounds.
			button.playAudioEvents = true;

			//set button position
			button.transformPosition = new Vector3(0.8f, 0.95f);
			button.BringToFront();

            try
			{
				//get the names of any districts in the city
				DistrictManager dm = DistrictManager.instance;

                // example for iterating through the structures
				int dCount = 0;
				uint maxDCount = dm.m_districts.m_size;

				//Debug.Log ("District maxDCount: " + maxDCount);
				for (int i = 0; i < maxDCount; i++) {
					String d = dm.GetDistrictName(i);
					if (d != null && ! d.Equals ("")) {
						dCount += 1;
					}
				}

			}catch (Exception e) {
				Debug.Log ("Error in detecting district names");
				Debug.Log (e.Message);
				Debug.Log (e.StackTrace);
			}

		}

	}

} 