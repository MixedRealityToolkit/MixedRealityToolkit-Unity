MRTK3 allows you to load the models of the controllers you are using, without the need to initialize or configure create any yourself.
It does so by loading the 3D model's data directly from the platform, however, these models are stored in a special, animation enabled format which
requires some additional packages.

MRTK3 does not take a hard dependency on these packages, but they are required to load these kind of models.

To add these packages to your project. go to
	Window -> Package Manager -> '+ icon' -> Add package by name...

glTFast: com.unity.cloud.gltfast
KTX: com.unity.cloud.ktx
