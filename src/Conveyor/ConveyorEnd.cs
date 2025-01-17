using Godot;
using System;

[Tool]
public partial class ConveyorEnd : Node3D
{
	public float Speed;
	double beltPosition = 0.0;
	bool running = false;

	StaticBody3D staticBody;
	MeshInstance3D mesh;
	public ShaderMaterial beltMaterial;
	Shader beltShader;

	IConveyor owner;
	Root main;

	float prevScaleX;

	public override void _Ready()
	{
		staticBody = GetNode<StaticBody3D>("StaticBody3D");

		mesh = GetNode<MeshInstance3D>("MeshInstance3D");
		mesh.Mesh = mesh.Mesh.Duplicate() as Mesh;
		beltMaterial = mesh.Mesh.SurfaceGetMaterial(0).Duplicate() as ShaderMaterial;
		mesh.Mesh.SurfaceSetMaterial(0, beltMaterial);
		beltShader = beltMaterial.Shader.Duplicate() as Shader;
		beltMaterial.Shader = beltShader;

		owner = Owner as IConveyor;
	}

	public override void _Process(double delta)
	{
		if (owner == null) return;

		if (owner.Scale.X != prevScaleX)
		{
			Scale = new Vector3(1 / owner.Scale.X, 1, 1);
			prevScaleX = owner.Scale.X;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (owner != null)
		{
			RemakeMesh(owner);

			if (owner.Main == null) return;

			if (owner.Main.Start)
			{
				Vector3 localFront = -GlobalTransform.Basis.Z.Normalized();
				beltPosition += Speed * delta;
				if (beltPosition >= 1.0)
					beltPosition = 0.0;
				staticBody.ConstantAngularVelocity = -localFront * Speed * MathF.PI;
			}
			else
			{
				beltPosition = 0; // Remove from here when signals are fixed.
				staticBody.ConstantAngularVelocity = Vector3.Zero;
			}

			((ShaderMaterial)beltMaterial).SetShaderParameter("BeltPosition", beltPosition * Mathf.Sign(-Speed));
		}
	}

	public void RemakeMesh(IConveyor conveyor)
	{
		if (conveyor.Speed != 0)
			((ShaderMaterial)beltMaterial).SetShaderParameter("Scale", Mathf.Sign(conveyor.Speed));
	}
}
