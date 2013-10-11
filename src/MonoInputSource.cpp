/*=============================================================================
Blobby Volley 2
Copyright (C) 2006 Jonathan Sieber (jonathan_sieber@yahoo.de)
Copyright (C) 2006 Daniel Knobe (daniel-knobe@web.de)

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; either version 2 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
=============================================================================*/

/* header include */
#include "MonoInputSource.h"

/* includes */
#include "DuelMatch.h"
#include "GameConstants.h"
#include "BotAPICalculations.h"
#include "FileRead.h"
#include "MonoLoader.h"

#include <iostream>
#include <SDL/SDL.h>
#include <cmath>
#include <algorithm>
#include <stdint.h>

/* implementation */

MonoInputSource::MonoInputSource(const std::string& filename,
						PlayerSide playerside, unsigned int difficulty)
{
	// Load the given bot from mono
	loader = &MonoLoader::getSingleton();
	const char* filePath = filename.c_str();
	unsigned int ticks= SDL_GetTicks();
	BotLoadInfo info;
	info.file = filePath;
	info.difficulty = difficulty;
	info.playerside = playerside;
	info.startTime =  ticks;
	
	// Loading constant game data
	info.CONST_FIELD_WIDTH=RIGHT_PLANE;
	info.CONST_GROUND_HEIGHT = 600 - GROUND_PLANE_HEIGHT_MAX;
	info.CONST_BALL_GRAVITY = -BALL_GRAVITATION;
	info.CONST_BALL_RADIUS = BALL_RADIUS;
	info.CONST_BLOBBY_JUMP = BLOBBY_JUMP_ACCELERATION;
	info.CONST_BLOBBY_BODY_RADIUS = BLOBBY_LOWER_RADIUS;
	info.CONST_BLOBBY_HEAD_RADIUS = BLOBBY_UPPER_RADIUS;
	info.CONST_BLOBBY_HEIGHT = BLOBBY_HEIGHT;
	info.CONST_BLOBBY_GRAVITY = -GRAVITATION;
	info.CONST_NET_HEIGHT = 600 - NET_SPHERE_POSITION;
	info.CONST_NET_RADIUS = NET_RADIUS;
	
	printf("Loading bot %s \n", filePath);
	botHandle = loader->loadBot(info);
	printf("bothandle loaded\n");
}

MonoInputSource::~MonoInputSource()
{
	// remove the bot
	printf("UnLoading bot\n");
	loader->unloadBot(botHandle);
}

MonoVector2 MonoInputSource::convertVector2(Vector2 vector){
	MonoVector2 v;
	v.x = vector.x;
	v.y = vector.y;
	return v;
}

PlayerInput MonoInputSource::getNextInput()
{	
	const DuelMatch* mMatch = getMatch();
	if (mMatch == 0)
	{
		return PlayerInput();
	}
	// Loading required variables
	
	WorldState state;
	state.ServingPlayer = mMatch->getServingPlayer();
	state.BallPosition = convertVector2(mMatch->getBallPosition());
	state.BallVelocity = convertVector2(mMatch->getBallVelocity());
	state.RightBlobPosition = convertVector2(mMatch->getBlobPosition(RIGHT_PLAYER));
	state.LeftBlobPosition = convertVector2(mMatch->getBlobPosition(LEFT_PLAYER));
	state.BallActive = mMatch->getBallActive();
	state.BallDown = mMatch->getBallDown();
	state.RightScore = mMatch->getScore(RIGHT_PLAYER);
	state.LeftScore = mMatch->getScore(LEFT_PLAYER);
	state.RightBlobJump = mMatch->getBlobJump(RIGHT_PLAYER);
	state.LeftBlobJump = mMatch->getBlobJump(LEFT_PLAYER);
	state.RightTouches = mMatch->getHitcount(RIGHT_PLAYER);
	state.LeftTouches = mMatch->getHitcount(LEFT_PLAYER);
	state.Tick = SDL_GetTicks();
		
	//printf("Getting input bot... \n");
	PlayerInput t = loader->getInput(botHandle, state);
	//printf("Got input\n");
	return t;
}

