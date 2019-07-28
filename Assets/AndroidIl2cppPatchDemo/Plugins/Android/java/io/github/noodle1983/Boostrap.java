package io.github.noodle1983;

public class Boostrap
{
    public static native void init(String filePath);
	
	public static void InitNativeLibBeforeUnityPlay(String filePath)
	{
		System.loadLibrary("main");
        System.loadLibrary("unity");
        System.loadLibrary("bootstrap");
        init(filePath);		
	}
}