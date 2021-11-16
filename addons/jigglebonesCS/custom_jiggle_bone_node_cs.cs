#if TOOLS
using Godot;
using System;

[Tool]
public class custom_jiggle_bone_node_cs : EditorPlugin
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

    }

    public override void _EnterTree()
    {
        //base._EnterTree();
        var jbscript = GD.Load<Script>("res://addons/jigglebonesCS/jigglebonecs.cs");
        var jbicon = GD.Load<Texture>("res://addons/jigglebonesCS/icon.svg");
        AddCustomType("JiggleboneCS", "Spatial", jbscript, jbicon);
    }

    public override void _ExitTree()
    {
        //base._ExitTree();
        RemoveCustomType("JiggleboneCS");

    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
#endif