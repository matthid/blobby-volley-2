/*=============================================================================
Mono Loader
Copyright (C) 2012 Matthias Dittrich (matthi.d@gmail.com)

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
#pragma once 

#include "InputSource.h"
#include "Global.h"

#include <mono/jit/jit.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/threads.h>
#include <stdint.h>

struct MonoVector2 {
	float x;
	float y;
};

struct WorldState {
	int8_t ServingPlayer;
	
	MonoVector2 BallPosition;
	MonoVector2 BallVelocity;
	MonoVector2 RightBlobPosition;
	MonoVector2 LeftBlobPosition;
	
	bool BallActive;
	bool BallDown;
	int32_t RightScore;
	int32_t LeftScore;
	bool RightBlobJump;
	bool LeftBlobJump;
	int32_t RightTouches;
	int32_t LeftTouches;
	
	uint32_t Tick;
};

struct BotLoadInfo {
	const char* file;
	int difficulty;
	unsigned int startTime;
	int8_t playerside;
	float CONST_FIELD_WIDTH;
	float CONST_GROUND_HEIGHT;
	float CONST_BALL_GRAVITY;
	float CONST_BALL_RADIUS;
	float CONST_BLOBBY_JUMP;
	float CONST_BLOBBY_BODY_RADIUS;
	float CONST_BLOBBY_HEAD_RADIUS;
	float CONST_BLOBBY_HEIGHT;
	float CONST_BLOBBY_GRAVITY;
	float CONST_NET_HEIGHT;
	float CONST_NET_RADIUS;
};
	
typedef MonoObject* (__attribute__((stdcall)) *LoadBotMethod) (MonoObject* loadInfo, MonoException ** exn);
typedef MonoObject* (__attribute__((stdcall)) *StepBotMethod) (MonoObject* bot, MonoObject* state, MonoException ** exn);
typedef MonoObject* (__attribute__((stdcall)) *UnloadBotMethod) (MonoObject* bot, MonoException ** exn);
class MonoLoader
{
public:
	static MonoLoader& getSingleton();
	void* loadBot(BotLoadInfo& info);
	PlayerInput getInput(void* bothandle, WorldState& status);
	void unloadBot(void* handle);
	
private:
	/// The constructor automatically loads and initializes the script
	/// with the given filename. The side parameter tells the script
	/// which side is it on.
	MonoLoader();
	~MonoLoader();
	
	MonoObject* boxWorldState(WorldState& state);
	MonoObject* boxLoadInfo(BotLoadInfo& info);
	PlayerInput unboxPlayerInput(MonoObject* playerInput);
	void handleExn(const char* info, MonoException* exn, bool logSuccess);
	
	MonoClass* playerInputClass;
	MonoClass* botLoaderInfoClass;
	MonoClass* worldStateInfoClass;
	MonoClass* exceptionClass;
	MonoProperty* exceptionMessage;
	MonoProperty* exceptionStacktrace;
	
	/// We only need 1 MonoLoader instance
	static MonoLoader* singleTon;
	MonoDomain *domain;
	MonoAssembly *assembly;
	LoadBotMethod loadMethod;
	StepBotMethod stepMethod;
	UnloadBotMethod unloadMethod;
	
	MonoMethod* loadMethodHandle;
	MonoMethod* stepMethodHandle;
	MonoMethod* unloadMethodHandle;
};
