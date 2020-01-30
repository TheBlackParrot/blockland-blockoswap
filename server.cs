//exec("./changelog.cs");
exec("./sounds.cs");
exec("./shapes.cs");

$GLOBAL_DELAY = 300;

$PRINT_NUM_START = 35;

// !! very unreadable code ahead !! sorry

function shuffleBoard() {
	for(%x = 0; %x < 8; %x++) {
		for(%y = 0; %y < 8; %y++) {
			%keepGoing = 1;
			while(%keepGoing) {
				%color = getRandom(0, 6);
				if(getFieldCount(checkMatches(%x, %y, %color)) < 2) {
					%keepGoing = false;
				}
			}

			$Server::Blockoswap::Px[%x, %y] = %color;
			if(isObject($Server::Blockoswap::Brick[%x, %y])) {
				$Server::Blockoswap::Brick[%x, %y].changePiece(%color);
			}
		}
	}
}

function scoreEmitter(%x, %y, %score) {
	for(%i = 0; %i < strLen(%score); %i++) {
		%c = getSubStr(%score, %i, 1);

		%pos = %x + (%i * 0.35) - (strLen(%score)/2*0.175) SPC -0.3 SPC %y-0.1;

		%emitter = new ParticleEmitterNode() {
			dataBlock = "GenericEmitterNode";
			emitter = "score" @ %c @ "Emitter";
			pointPlacement = 0;
			position = %pos;
			rotation = "1 0 0 0";
			scale = "0.05 0.05 0.05";
			spherePlacement = 0;
			velocity = "1";
		};

		MissionCleanup.add(%emitter);

		%emitter.schedule(150, delete);
	}
}

function SimGroup::initBoard(%this) {
	%offset = "0 0 0.6";
	%brickType = "brick1x1Data";

	shuffleBoard();

	for(%x = 0; %x < 8; %x++) {
		for(%y = 0; %y < 8; %y++) {
			%pos = %x * (%brickType.brickSizeX/2) SPC 0 SPC %y * (%brickType.brickSizeZ/5);
			$Server::Blockoswap::Brick[%x, %y] = %brick = new fxDTSBrick() {
				angleID = 0;
				client = %this.client;
				colorFxID = 0;
				colorID = $Server::Blockoswap::Px[%x, %y];
				dataBlock = getJewelDatablockFromColor($Server::Blockoswap::Px[%x, %y]);
				position = vectorAdd(%pos, %offset);
				rotation = "0 0 1 90";
				scale = "1 1 1";
				shapeFxID = 0;
				stackBL_ID = -1;
				isBasePlate = 0;
				isPlanted = 1;
				printID = 0;
				x = %x;
				y = %y;
				active = 0;
			};

			%this.add(%brick);
			%brick.setTrusted(1);
			%brick.plant();
		}
	}

	for(%x = 0; %x < 6; %x++) {
		$Server::Blockoswap::ScoreBrick[%x] = %brick = new fxDTSBrick() {
			angleID = 0;
			client = -1;
			colorFxID = 0;
			colorID = 7;
			dataBlock = "brick1x1PrintData";
			position = vectorAdd("-0.75 0.25 4.3", -0.5 * %x SPC 0 SPC 0);
			rotation = "1 0 0 0";
			scale = "1 1 1";
			shapeFxID = 0;
			stackBL_ID = -1;
			isBasePlate = 0;
			isPlanted = 1;
			printID = (%x == 0 ? 35 : 36);
		};

		%this.add(%brick);
		%brick.setTrusted(1);
		%brick.plant();
	}

	for(%x = 0; %x < 6; %x++) {
		switch(%x) {
			case 0: %pID = 34;
			case 4: %pID = 4;
			case 5: %pID = 14;
			default: %pID = 36;
		}
		$Server::Blockoswap::LevelBrick[%x] = %brick = new fxDTSBrick() {
			angleID = 0;
			client = -1;
			colorFxID = 0;
			colorID = 8;
			dataBlock = "brick1x1PrintData";
			position = vectorAdd("-0.75 0.25 3.7", -0.5 * %x SPC 0 SPC 0);
			rotation = "1 0 0 0";
			scale = "1 1 1";
			shapeFxID = 0;
			stackBL_ID = -1;
			isBasePlate = 0;
			isPlanted = 1;
			printID = %pID;
		};

		%this.add(%brick);
		%brick.setTrusted(1);
		%brick.plant();
	}

	if(isObject(%this.levelCompletionShape)) {
		%this.levelCompletionShape.delete();
	}
	%this.levelCompletionShape = new StaticShape(BlockoswapLevelCompletion) {
		dataBlock = CubeShape;
		position = "0.25 0.033 0.2";
		scale = 0 SPC 0.1 SPC 0.15;
	};

	%client = %this.client;
	%client.level = 1;
	%client.score = 0;
	%client.scoreInLevel = 0;
	%client.canMove = 1;
}

function SimGroup::setLevelProgress(%this, %perc) {
	%this.levelCompletionShape.setScale((%perc/100) * 8 SPC 0.1 SPC 0.15);
	%this.levelCompletionShape.setTransform((%perc/100) * 2 SPC 0.033 SPC 0.2);
}

function checkMatches(%x, %y, %color) {
	%matchesX = checkBoardInDirection(%x, %y, 1, 0, %color);
	//talk("mX:" SPC strReplace(%matchesX, "\t", ", "));
	%matchesX = trim(%matchesX TAB checkBoardInDirection(%x, %y, -1, 0, %color));
	//talk("mX:" SPC strReplace(%matchesX, "\t", ", "));

	%matchesY = checkBoardInDirection(%x, %y, 0, 1, %color);
	//talk("mY:" SPC strReplace(%matchesY, "\t", ", "));
	%matchesY = trim(%matchesY TAB checkBoardInDirection(%x, %y, 0, -1, %color));
	//talk("mY:" SPC strReplace(%matchesY, "\t", ", "));

	if(getFieldCount(%matchesX) >= 2) {
		%matches = %matchesX;
	}
	if(getFieldCount(%matchesY) >= 2) {
		%matches = trim(%matches TAB %matchesY);
	}

	return %matches;
}

function checkBoardInDirection(%x, %y, %xDir, %yDir, %color) {
	%keepGoing = 1;

	while(%keepGoing) {
		%x += %xDir;
		%y += %yDir;

		%checking = $Server::Blockoswap::Px[%x, %y];

		if(%x < 0 || %y < 0 || %x >= 8 || %y >= 8 || %color != %checking) {
			%keepGoing = 0;
		} else {
			%matches = trim(%matches TAB %x SPC %y);
		}
	}

	return %matches;
}

function serverCmdTestBoard(%client) {
	%client.brickgroup.chainDeleteCallback = "BrickGroup_999999.initBoard();";
	%client.brickgroup.chainDeleteAll();
}

function fxDTSBrick::setActive(%this, %client, %tog) {
	if(!%client.canMove) {
		%client.play2D(cannotSwap);
		return;
	}

	if(%tog) {
		%this.setColorFX(3);
		%this.active = true;
		%client.activeX = %this.x;
		%client.activeY = %this.y;
	} else {
		%this.setColorFX(0);
		%this.active = false;
		%client.activeX = "";
		%client.activeY = "";		
	}
}

function getJewelDatablockFromColor(%color) {
	if(%color == 63) {
		return "brick1x1Data".getID();
	}

	%ret = "red";

	switch(%color) {
		case 1: %ret = "orange";
		case 2: %ret = "yellow";
		case 3: %ret = "green";
		case 4: %ret = "blue";
		case 5: %ret = "pink";
		case 6: %ret = "white";
	}

	return (%ret @ "JewelData").getID();
}

function fxDTSBrick::changePiece(%this, %color) {
	$Server::Blockoswap::Px[%this.x, %this.y] = %color;
	%this.setColor(%color);

	$Server::Blockoswap::Brick[%this.x, %this.y] = new fxDTSBrick() {
		angleID = 0;
		client = %this.client;
		colorFxID = 0;
		colorID = $Server::Blockoswap::Px[%this.x, %this.y];
		dataBlock = getJewelDatablockFromColor($Server::Blockoswap::Px[%this.x, %this.y]);
		position = %this.getPosition();
		rotation = "0 0 1 90";
		scale = "1 1 1";
		shapeFxID = 0;
		stackBL_ID = -1;
		isBasePlate = 0;
		isPlanted = 1;
		printID = 0;
		x = %this.x;
		y = %this.y;
		active = 0;
	};
	//talk($Server::Blockoswap::Brick[%this.x, %this.y]);

	%x = %this.x;
	%y = %this.y;
	schedule(1, 0, plantNewBrick, $Server::Blockoswap::Brick[%x, %y]);

	%this.delete();
}

function plantNewBrick(%brick) {
	if(!isObject(%brick)) {
		// the fuck?
		return;
	}
	%brick.client.brickgroup.add($Server::Blockoswap::Brick[%brick.x, %brick.y]);
	$Server::Blockoswap::Brick[%brick.x, %brick.y].setTrusted(1);
	$Server::Blockoswap::Brick[%brick.x, %brick.y].plant();	
}

function swapPieces(%wantedBrick, %originBrick, %client) {
	%x1 = %originBrick.x;
	%y1 = %originBrick.y;
	%c1 = $Server::Blockoswap::Px[%x1, %y1];
	%x2 = %wantedBrick.x;
	%y2 = %wantedBrick.y;
	%c2 = $Server::Blockoswap::Px[%x2, %y2];

	%wantedBrick.changePiece(%c1);
	%originBrick.changePiece(%c2);

	%client.canMove = 0;

	schedule($GLOBAL_DELAY, 0, postSwap, %x1, %y1, %c1, %x2, %y2, %c2, %client);
}

function GameConnection::incrScore(%client, %amount) {
	%client.score += %amount;
	%client.scoreInLevel += %amount;

	%client.scoreToAnimate += %amount;
	%client.animateScore();

	%maxToProgress = %client.level * 500;
	if(%client.scoreInLevel > %maxToProgress) {
		%client.scoreInLevel = %maxToProgress;
		%client.moveToNextLevel = 1;
	}
}

function GameConnection::animateScore(%client) {
	cancel(%client.scoreToAnimateSched);

	%at = %client.score - %client.scoreToAnimate;
	%client.scoreToAnimate -= mCeil(%client.scoreToAnimate/10);

	%maxToProgress = %client.level * 500;

	if(%client.scoreToAnimate >= 1) {
		%client.scoreToAnimateSched = %client.schedule(67, animateScore);
	} else {
		%client.brickgroup.setLevelProgress((%client.scoreInLevel / %maxToProgress)*100);

		for(%i = 0; %i < strLen(%client.score); %i++) {
			%c = getSubStr(%client.score, strLen(%client.score)-%i-1, 1);
			$Server::Blockoswap::ScoreBrick[%i].setPrint($PRINT_NUM_START - %c);
		}

		return;
	}

	%client.brickgroup.setLevelProgress(((%client.scoreInLevel - %client.scoreToAnimate) / %maxToProgress)*100);

	for(%i = 0; %i < strLen(%at); %i++) {
		%c = getSubStr(%at, strLen(%at)-%i-1, 1);
		$Server::Blockoswap::ScoreBrick[%i].setPrint($PRINT_NUM_START - %c);
	}
}

function GameConnection::nextLevel(%client) {
	%client.scoreInLevel = 0;
	%client.level++;
	%client.brickgroup.setLevelProgress(0);
	%client.moveToNextLevel = 0;
	shuffleBoard();

	%client.play2D(levelComplete);
	%client.schedule(750, play2D, levelCompleteVoice);

	for(%i = 0; %i < strLen(%client.level); %i++) {
		%c = getSubStr(%client.level, %i, 1);
		$Server::Blockoswap::LevelBrick[%i].setPrint($PRINT_NUM_START - %c);
	}

	for(%i = 0; %i < 6; %i++) {
		$Server::Blockoswap::LevelBrick[%i].setColorFX(3);
		$Server::Blockoswap::LevelBrick[%i].schedule(250, setColorFX, 0);
		$Server::Blockoswap::LevelBrick[%i].schedule(500, setColorFX, 3);
		$Server::Blockoswap::LevelBrick[%i].schedule(750, setColorFX, 0);
		$Server::Blockoswap::LevelBrick[%i].schedule(1000, setColorFX, 3);
		$Server::Blockoswap::LevelBrick[%i].schedule(1250, setColorFX, 0);
	}
}

function postSwap(%x1, %y1, %c1, %x2, %y2, %c2, %client) {
	%matchesWanted = trim(%x2 SPC %y2 TAB checkMatches(%x2, %y2, %c1));
	talk("matchesWanted:" SPC strReplace(%matchesWanted, "\t", ", "));
	%matchesOrigin = trim(%x1 SPC %y1 TAB checkMatches(%x1, %y1, %c2));
	talk("matchesOrigin:" SPC strReplace(%matchesOrigin, "\t", ", "));

	if(getFieldCount(%matchesWanted) > 2) {
		%matches = %matchesWanted;
	}
	if(getFieldCount(%matchesOrigin) > 2) {
		%matches = trim(%matches TAB %matchesOrigin);
	}

	if(getFieldCount(%matches) > 2) {
		talk("match found with length of" SPC getFieldCount(%matches) @ ":" SPC strReplace(%matches, "\t",  ", "));
		if(getFieldCount(%matches) > 3) {
			%client.play2D(doubleMatchFound);
		} else {
			%client.play2D(matchFound);
		}

		%scoreToAdd = (10 + (5 * (getFieldCount(%matches) - 3))) + (5 * (%client.level - 1));
		%client.incrScore(%scoreToAdd);

		%highX = -999999;
		%highY = -999999;
		%lowX = 999999;
		%lowY = 999999;
		for(%i = 0; %i < getFieldCount(%matches); %i++) {
			%match = getField(%matches, %i);
			%mX = getWord(%match, 0);
			%mY = getWord(%match, 1);

			$Server::Blockoswap::Brick[%mX, %mY].setToFall = 1;

			%bpos = $Server::Blockoswap::Brick[%mX, %mY].getPosition();
			%bX = getWord(%bpos, 0);
			%bY = getWord(%bpos, 2);

			if(%bX > %highX) { %highX = %bX; }
			if(%bY > %highY) { %highY = %bY; }
			if(%bX < %lowX) { %lowX = %bX; }
			if(%bY < %lowY) { %lowY = %bY; }
		}

		makeBoardFall("", 0, %client);

		talk(%lowX SPC %lowY SPC %highX SPC %highY);

		scoreEmitter((%lowX + %highX)/2, (%lowY + %highY)/2, %scoreToAdd);
	} else {
		talk("no matches");
		%client.play2D(swapBad);
		schedule($GLOBAL_DELAY, 0, swapFailed, %x1, %y1, %c1, %x2, %y2, %c2);
		%client.finishMoving();
	}
}

function swapFailed(%x1, %y1, %c1, %x2, %y2, %c2) {
	$Server::Blockoswap::Brick[%x1, %y1].changePiece(%c1);
	$Server::Blockoswap::Brick[%x2, %y2].changePiece(%c2);
}

function makeBoardFall(%skipChecks, %cc, %client) {
	%fallNext = false;
	if(%skipChecks $= "") {
		for(%x = 0; %x < 8; %x++) {
			for(%y = 0; %y < 8; %y++) {
				%brick = $Server::Blockoswap::Brick[%x, %y];

				if(!%brick.setToFall) {
					continue;
				}

				%fallNext = true;

				%brick.changePiece(63);
			}
		}

		if(%fallNext) {
			schedule($GLOBAL_DELAY, 0, makeBoardFall, 1, %cc, %client);
			return;
		}
	}

	talk("falling");
	%nextStep = true;

	for(%x = 0; %x < 8; %x++) {
		for(%y = 0; %y < 8; %y++) {
			%brick = $Server::Blockoswap::Brick[%x, %y];
			%nextBrick = $Server::Blockoswap::Brick[%x, %y-1];

			if(%y == 0) {
				continue;
			}

			if(%nextBrick.colorID == 63) {
				%nextStep = false;
				%nextBrick.changePiece(%brick.colorID);
				%brick.setToFall = false;
				%nextBrick.setToFall = false;

				for(%z = %y; %z < 8; %z++) {
					%temp_b = $Server::Blockoswap::Brick[%x, %z];
					%temp_b_up = $Server::Blockoswap::Brick[%x, %z+1];

					if(%z >= 7) {
						%temp_b.changePiece(getRandom(0, 6));
					} else {
						%temp_b.changePiece(%temp_b_up.colorID);
					}
				}

				schedule($GLOBAL_DELAY, 0, makeBoardFall, 1, %cc, %client);
			} else if(%y == 7 && %brick.colorID == 63) {
				%nextStep = false;
				%brick.changePiece(getRandom(0, 6));
				%brick.setToFall = false;
				%nextBrick.setToFall = false;

				schedule($GLOBAL_DELAY, 0, makeBoardFall, 1, %cc, %client);
			}
		}
	}

	if(%nextStep && !isEventPending($Server::Blockoswap::ComboSched)) {
		$Server::Blockoswap::ComboSched = schedule($GLOBAL_DELAY, 0, checkBoardForCombos, %cc++, %client);
	}
}

function checkBoardForCombos(%cc, %client) {
	cancel($Server::Blockoswap::ComboSched);
	talk("checking for combos");
	%fall = false;

	%highX = -999999;
	%lowX = 999999;
	%highY = -999999;
	%lowY = 999999;
	for(%x = 0; %x < 8; %x++) {
		for(%y = 0; %y < 8; %y++) {
			%brick = $Server::Blockoswap::Brick[%x, %y];
			%matches = trim(%x SPC %y TAB checkMatches(%x, %y, %brick.colorID));
			if(getFieldCount(%matches) > 2) {
				for(%i = 0; %i < getFieldCount(%matches); %i++) {
					%match = getField(%matches, %i);
					%mX = getWord(%match, 0);
					%mY = getWord(%match, 1);

					$Server::Blockoswap::Brick[%mX, %mY].setToFall = 1;

					if(%uniqueMatch[%mX, %mY] $= "") {
						%uniqueMatch[%mX, %mY] = 1;
						%uniqueMatches = trim(%uniqueMatches TAB %mX SPC %mY);
					}

					%bpos = $Server::Blockoswap::Brick[%mX, %mY].getPosition();
					%bX = getWord(%bpos, 0);
					%bY = getWord(%bpos, 2);

					if(%bX > %highX) { %highX = %bX; }
					if(%bY > %highY) { %highY = %bY; }
					if(%bX < %lowX) { %lowX = %bX; }
					if(%bY < %lowY) { %lowY = %bY; }
				}

				%allMatches = trim(%allMatches TAB %matches);

				%fall = true;
			}
		}
	}

	if(%fall) {
		talk("FALLING" SPC %cc SPC getFieldCount(%uniqueMatches));
		makeBoardFall("", %cc, %client);

		%scoreToAdd = ((10 + (5 * (getFieldCount(%uniqueMatches) - 3))) + (5 * (%client.level - 1))) * (%cc+1);
		%client.incrScore(%scoreToAdd);
		scoreEmitter((%lowX + %highX)/2, (%lowY + %highY)/2, %scoreToAdd);

		if(%cc > 6) {
			%client.play2D(combo6);
		} else {
			%client.play2D("combo" @ %cc);
		}
	} else {
		%client.finishMoving();
	}
}

function GameConnection::finishMoving(%client) {
	if(%client.moveToNextLevel) {
		%client.nextLevel();
	}

	%client.canMove = 1;

	%dirs = "0 1\t0 -1\t1 0\t-1 0";

	%matchFound = false;
	for(%x = 0; %x < 8; %x++) {
		for(%y = 0; %y < 8; %y++) {
			$Server::Blockoswap::Brick[%x, %y].setToFall = 0;

			%t1 = $Server::Blockoswap::Px[%x, %y];
			%t2 = $Server::Blockoswap::Px[%x+1, %y];
			if(%t2 !$= "") {
				$Server::Blockoswap::Px[%x, %y] = %t2;
				$Server::Blockoswap::Px[%x+1, %y] = %t1;
				%matches = trim(%x SPC %y TAB checkMatches(%x, %y, $Server::Blockoswap::Px[%x, %y]));
				$Server::Blockoswap::Px[%x, %y] = %t1;
				$Server::Blockoswap::Px[%x+1, %y] = %t2;
				if(getFieldCount(%matches) > 2) {
					talk("potential match (x+1) found at" SPC %x SPC %y SPC "--" SPC %matches);
					%matchFound = 1;
					break;
				}
			}

			%t1 = $Server::Blockoswap::Px[%x, %y];
			%t2 = $Server::Blockoswap::Px[%x-1, %y];
			if(%t2 !$= "") {
				$Server::Blockoswap::Px[%x, %y] = %t2;
				$Server::Blockoswap::Px[%x-1, %y] = %t1;
				%matches = trim(%x SPC %y TAB checkMatches(%x, %y, $Server::Blockoswap::Px[%x, %y]));
				$Server::Blockoswap::Px[%x, %y] = %t1;
				$Server::Blockoswap::Px[%x-1, %y] = %t2;
				if(getFieldCount(%matches) > 2) {
					talk("potential match (x-1) found at" SPC %x SPC %y SPC "--" SPC %matches);
					%matchFound = 1;
					break;
				}
			}

			%t1 = $Server::Blockoswap::Px[%x, %y];
			%t2 = $Server::Blockoswap::Px[%x, %y+1];
			if(%t2 !$= "") {
				$Server::Blockoswap::Px[%x, %y] = %t2;
				$Server::Blockoswap::Px[%x, %y+1] = %t1;
				%matches = trim(%x SPC %y TAB checkMatches(%x, %y, $Server::Blockoswap::Px[%x, %y]));
				$Server::Blockoswap::Px[%x, %y] = %t1;
				$Server::Blockoswap::Px[%x, %y+1] = %t2;
				if(getFieldCount(%matches) > 2) {
					talk("potential match (y+1) found at" SPC %x SPC %y SPC "--" SPC %matches);
					%matchFound = 1;
					break;
				}
			}

			%t1 = $Server::Blockoswap::Px[%x, %y];
			%t2 = $Server::Blockoswap::Px[%x, %y-1];
			if(%t2 !$= "") {
				$Server::Blockoswap::Px[%x, %y] = %t2;
				$Server::Blockoswap::Px[%x, %y-1] = %t1;
				%matches = trim(%x SPC %y TAB checkMatches(%x, %y, $Server::Blockoswap::Px[%x, %y]));
				$Server::Blockoswap::Px[%x, %y] = %t1;
				$Server::Blockoswap::Px[%x, %y-1] = %t2;
				if(getFieldCount(%matches) > 2) {
					talk("potential match (y-1) found at" SPC %x SPC %y SPC "--" SPC %matches);
					%matchFound = 1;
					break;
				}
			}
		}
		if(%matchFound) {
			break;
		}
	}

	if(!%matchFound) {
		%client.canMove = 0;
		%client.play2D(noMoreMoves);
		%client.brickgroup.levelCompletionShape.setNodeColor("ALL", "1 0 0 1");
	}
}

package BlockoswapPackage {
	function fxDTSBrick::onActivate(%this, %player, %client, %c, %d) {
		%r = parent::onActivate(%this, %player, %client, %c, %d);

		if(%this.x $= "") {
			return %r;
		}

		%client.play2D(select);

		if(%this.active) {
			%this.setActive(%client, 0);
		} else {
			if(%client.activeX !$= "") {
				%dir = %client.activeX - %this.x SPC %client.activeY - %this.y;

				switch$(%dir) {
					case "0 -1" or "0 1" or "-1 0" or "1 0":
						%brickInQuestion = $Server::Blockoswap::Brick[%client.activeX, %client.activeY];
						%this.setActive(%client, 0);
						%brickInQuestion.setActive(%client, 0);

						swapPieces(%this, %brickInQuestion, %client);
						return %r;

					case "0 0":
						%this.setActive(%client, 0);
						return %r;

					default:
						%client.play2D(cannotSwap);
						return %r;

				}
			}
			%this.setActive(%client, 1);
		}

		return %r;
	}
};
activatePackage(BlockoswapPackage);