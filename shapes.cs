datablock StaticShapeData(CubeShape) {
	shapeFile = "./shapes/cube.dts";
};

datablock fxDTSBrickData(redJewelData) {
	brickFile = "./shapes/redjewel.blb";
	category = "Blockoswap";
	subCategory = "Gems";
	uiName = "Red Jewel";
};
datablock fxDTSBrickData(orangeJewelData : redJewelData) {
	brickFile = "./shapes/orangejewel.blb";
	uiName = "Orange Jewel";
};
datablock fxDTSBrickData(yellowJewelData : redJewelData) {
	brickFile = "./shapes/yellowjewel.blb";
	uiName = "Yellow Jewel";
};
datablock fxDTSBrickData(greenJewelData : redJewelData) {
	brickFile = "./shapes/greenjewel.blb";
	uiName = "Green Jewel";
};
datablock fxDTSBrickData(blueJewelData : redJewelData) {
	brickFile = "./shapes/bluejewel.blb";
	uiName = "Blue Jewel";
};
datablock fxDTSBrickData(pinkJewelData : redJewelData) {
	brickFile = "./shapes/pinkjewel.blb";
	uiName = "Pink Jewel";
};
datablock fxDTSBrickData(whiteJewelData : redJewelData) {
	brickFile = "./shapes/whitejewel.blb";
	uiName = "White Jewel";
};

datablock ParticleData(score0Particle) {
	dragCoefficient      = 0;
	gravityCoefficient   = 0.0;
	inheritedVelFactor   = 0.7;
	constantAcceleration = 0.1;
	lifetimeMS           = 800;
	lifetimeVarianceMS   = 0;
	textureName          = "Add-Ons/Print_Letters_Continuum/prints/0";
	spinSpeed		= 0.0;
	spinRandomMin		= -0.0;
	spinRandomMax		= 0.0;
	colors[0]     = "1 1 1 1";
	colors[1]     = "1 1 1 0";
	sizes[0]      = 0.5;
	sizes[1]      = 0.5;
};
datablock ParticleData(score1Particle : score0Particle) { textureName = "Add-Ons/Print_Letters_Continuum/prints/1"; };
datablock ParticleData(score2Particle : score0Particle) { textureName = "Add-Ons/Print_Letters_Continuum/prints/2"; };
datablock ParticleData(score3Particle : score0Particle) { textureName = "Add-Ons/Print_Letters_Continuum/prints/3"; };
datablock ParticleData(score4Particle : score0Particle) { textureName = "Add-Ons/Print_Letters_Continuum/prints/4"; };
datablock ParticleData(score5Particle : score0Particle) { textureName = "Add-Ons/Print_Letters_Continuum/prints/5"; };
datablock ParticleData(score6Particle : score0Particle) { textureName = "Add-Ons/Print_Letters_Continuum/prints/6"; };
datablock ParticleData(score7Particle : score0Particle) { textureName = "Add-Ons/Print_Letters_Continuum/prints/7"; };
datablock ParticleData(score8Particle : score0Particle) { textureName = "Add-Ons/Print_Letters_Continuum/prints/8"; };
datablock ParticleData(score9Particle : score0Particle) { textureName = "Add-Ons/Print_Letters_Continuum/prints/9"; };
datablock ParticleEmitterData(score0Emitter) {
	ejectionPeriodMS = 100;
	periodVarianceMS = 0;
	ejectionVelocity = 0.025;
	velocityVariance = 0;
	ejectionOffset = 0;
	thetaMin = 0;
	thetaMax = 0;
	particles = score0Particle;
};
datablock ParticleEmitterData(score1Emitter : score0Emitter) { particles = score1Particle; };
datablock ParticleEmitterData(score2Emitter : score0Emitter) { particles = score2Particle; };
datablock ParticleEmitterData(score3Emitter : score0Emitter) { particles = score3Particle; };
datablock ParticleEmitterData(score4Emitter : score0Emitter) { particles = score4Particle; };
datablock ParticleEmitterData(score5Emitter : score0Emitter) { particles = score5Particle; };
datablock ParticleEmitterData(score6Emitter : score0Emitter) { particles = score6Particle; };
datablock ParticleEmitterData(score7Emitter : score0Emitter) { particles = score7Particle; };
datablock ParticleEmitterData(score8Emitter : score0Emitter) { particles = score8Particle; };
datablock ParticleEmitterData(score9Emitter : score0Emitter) { particles = score9Particle; };