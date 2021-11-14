using Godot;
using System;

[Tool]
public class jigglebonecs : Spatial
{
    public enum Axis
    {
        X_Plus, Y_Plus, Z_Plus, X_Minus, Y_Minus, Z_Minus
    }

    [Export] public String bone_name;
    [Export] public float stiffness = 1;
    [Export] public float damping = 0;
    [Export] bool use_gravity = false;
    [Export] public Vector3 gravity = new Vector3(0, -9.81f, 0);
    [Export] public Axis forward_axis = Axis.Z_Minus;
    [Export] public NodePath findSkeleton;
    public Skeleton skeleton;

    // Previous position
    Vector3 initial_translate;
    Vector3 prev_pos = new Vector3();
    // Rest length of the distance constraint
    float rest_length = 1;

    public override void _Ready()
    {
        SetAsToplevel(true);
        prev_pos = GlobalTransform.origin;
        skeleton = (Skeleton)GetParent();
        initial_translate = Translation;//new Vector3();//Translation();//Translation;
        if (skeleton == null)
        {
            skeleton = (Skeleton)GetNode(findSkeleton);
        }
    }
    public void Jiggleprint(String text)
    {
        GD.Print("[JigglebonesCS] Error: " + text);
    }
    public Vector3 Get_bone_forward_local()
    {

        switch (forward_axis)
        {
            case Axis.X_Plus: return new Vector3(1, 0, 0);
            case Axis.Y_Plus: return new Vector3(0, 1, 0);
            case Axis.Z_Plus: return new Vector3(0, 0, 1);
            case Axis.X_Minus: return new Vector3(-1, 0, 0);
            case Axis.Y_Minus: return new Vector3(0, -1, 0);
            case Axis.Z_Minus: return new Vector3(0, 0, -1);
        }
        return new Vector3(0, 0, 0);
    }
    public override void _Process(float delta)
    {
        if (!(skeleton is Skeleton))
        {
            Jiggleprint("Jigglebone must be a direct child of a Skeleton node");
            return;
        }

        if (bone_name == null)
        {
            Jiggleprint("Please enter a bone name");
            return;
        }

        var bone_id = skeleton.FindBone(bone_name);
        if (bone_id == -1)
        {
            Jiggleprint("Unknown bone " + bone_name + " - please enter a valid bone name");
            return;
        }
        else if (bone_id == 0)
        {
            Jiggleprint("0 place error bone " + bone_name + " reads it as negative 1");
            return;
        }
        int bone_id_parent = skeleton.GetBoneParent(bone_id);

        //Note:
        // Local space = local to the bone
        // Object space = local to the skeleton (confusingly called "global" in get_bone_global_pose)
        // World space = global
        // See https://godotengine.org/qa/7631/armature-differences-between-bones-custom_pose-transform

        Transform bone_transf_obj = skeleton.GetBoneGlobalPose(bone_id); //# Object space bone pose
        Transform bone_transf_world = skeleton.GlobalTransform * bone_transf_obj;
        Transform bone_transf_rest_local = skeleton.GetBoneRest(bone_id);
        Transform bone_transf_rest_obj = skeleton.GetBoneGlobalPose(bone_id_parent) * bone_transf_rest_local;
        Transform bone_transf_rest_world = skeleton.GlobalTransform * bone_transf_rest_obj;

        //############### Integrate velocity (Verlet integration) ##############	

        // If not using gravity, apply force in the direction of the bone (so it always wants to point "forward")
        Vector3 v = new Vector3(0, 0, -1);
        Vector3 grav = bone_transf_rest_world.basis.Xform(v).Normalized() * 9.81f;
        Vector3 vel = (GlobalTransform.origin - prev_pos) / delta;
        if (use_gravity)
        {
            grav = gravity;
        }
        grav *= stiffness;
        vel += grav;
        vel -= vel * damping * delta;  // Damping
        prev_pos = GlobalTransform.origin;
        Transform origintemp = GlobalTransform;
        origintemp.origin = origintemp.origin + vel * delta;
        GlobalTransform = origintemp;

        if((Translation.x != Translation.x) || (float.IsInfinity(Translation.x)) || (float.IsNegativeInfinity(Translation.x)))
        {
            Translation = new Vector3(initial_translate.x, Translation.y, Translation.z);
        }
        if ((Translation.y != Translation.y) || (float.IsInfinity(Translation.y)) || (float.IsNegativeInfinity(Translation.y)))
        {
            Translation = new Vector3(Translation.x, initial_translate.y, Translation.z);
        }
        if ((Translation.z != Translation.z) || (float.IsInfinity(Translation.z)) || (float.IsNegativeInfinity(Translation.z)))
        {
            Translation = new Vector3(Translation.x, Translation.y, initial_translate.z);
        }

        //Translation = initial_translate;
        //Translation = new Vector3(0, 0, 0);
        //############### Solve distance constraint ##############
        Vector3 goal_pos = skeleton.ToGlobal(skeleton.GetBoneGlobalPose(bone_id).origin);
        Vector3 new_pos_clamped = goal_pos + (GlobalTransform.origin - goal_pos).Normalized() * rest_length;

        origintemp = GlobalTransform;
        origintemp.origin = new_pos_clamped;
        GlobalTransform = origintemp;

        //############## Rotate the bone to point to this object #############
        Vector3 diff_vec_local = bone_transf_world.AffineInverse().Xform(GlobalTransform.origin).Normalized();
        Vector3 bone_forward_local = Get_bone_forward_local();

        // The axis+angle to rotate on, in local-to-bone space
        Vector3 bone_rotate_axis = bone_forward_local.Cross(diff_vec_local);
        float bone_rotate_angle = Mathf.Acos(bone_forward_local.Dot(diff_vec_local));

        if (bone_rotate_axis.Length() < 1e-3)
        {
            return;  // Already aligned, no need to rotate
        }
        bone_rotate_axis = bone_rotate_axis.Normalized();
        // Bring the axis to object space, WITHOUT translation (so only the BASIS is used) since vectors shouldn't be translated
        var bone_rotate_axis_obj = bone_transf_obj.basis.Xform(bone_rotate_axis).Normalized();
        var bone_new_transf_obj = new Transform(bone_transf_obj.basis.Rotated(bone_rotate_axis_obj, bone_rotate_angle), bone_transf_obj.origin);

        //if (bone_new_transf_obj[0][0] != null)        {            bone_new_transf_obj = new Transform();        }  // # Corrupted somehow
        if (( bone_new_transf_obj[0][0] != bone_new_transf_obj[0][0])) { bone_new_transf_obj = new Transform(); }

        skeleton.SetBoneGlobalPoseOverride(bone_id, bone_new_transf_obj, 0.5f, true);
        // Orient this object to the jigglebone
        origintemp = GlobalTransform;
        origintemp.basis = (skeleton.GlobalTransform * skeleton.GetBoneGlobalPose(bone_id)).basis;
        GlobalTransform = origintemp;
    }
}
