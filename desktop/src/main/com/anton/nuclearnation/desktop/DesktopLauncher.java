package com.anton.nuclearnation.desktop;

import com.badlogic.gdx.Files;
import com.badlogic.gdx.backends.lwjgl.LwjglApplication;
import com.badlogic.gdx.backends.lwjgl.LwjglApplicationConfiguration;
import com.anton.nuclearnation.NuclearNation;

public class DesktopLauncher {
	public static void main (String[] arg) {
		LwjglApplicationConfiguration config = new LwjglApplicationConfiguration();
		config.title = "Nuclear Nation";
		config.width = 1600;
		config.height = 960;
		config.addIcon("desktop/resources/nn-128x128.png", Files.FileType.Internal);
		config.addIcon("desktop/resources/nn-32x32.png", Files.FileType.Internal);
		config.addIcon("desktop/resources/nn-16x16.png", Files.FileType.Internal);
		new LwjglApplication(new NuclearNation(), config);
	}
}
