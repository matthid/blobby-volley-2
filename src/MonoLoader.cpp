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

#include "MonoLoader.h"

#include <iostream>

MonoLoader* MonoLoader::singleTon = 0;

MonoLoader& MonoLoader::getSingleton()
{
	if (!singleTon){
		
		singleTon = new MonoLoader;
	}
	return *singleTon;
}

#define MonoLoader_LoadHelper(toSet, methodHandle, type, name) \
	desc = mono_method_desc_new(name, true); \
	methodHandle = mono_method_desc_search_in_image(desc, image); \
	toSet = (type)mono_method_get_unmanaged_thunk (methodHandle); \
	mono_method_desc_free(desc); \
	

	
MonoLoader::MonoLoader(){

	printf("set dirs\n");
	mono_set_dirs("mono/lib","mono/etc");
	mono_set_dirs("C:\\Program Files (x86)\\Mono-3.2.3\\lib", "C:\\Program Files (x86)\\Mono-3.2.3\\etc");
	//mono_set_dirs("/cygdrive/c/Program Files (x86)/Mono-3.2.3/mono/lib", "/cygdrive/c/Program Files (x86)/Mono-3.2.3/mono/etc");
	printf("init domain\n");
	domain = mono_jit_init_version ("BOT_DOMAIN", "v4.0.30319"); 
	mono_set_assemblies_path ("mono:mono/lib:mono/lib/mono:mono/lib/mono/4.5:MonoBots");
	printf("open assembly\n");
	mono_thread_attach (domain);
	assembly = mono_domain_assembly_open(domain, "BlobbyBotLoader.dll");
	if (!assembly){
		std::cerr << "Mono Error: " << "Couldn't load assembly (BlobbyBotLoader.dll) \n";
	}
	//mono_config_parse (NULL);
	
	// mono_set_dirs("C:\\Mono-2.6.7\\lib","C:\\Mono-2.6.7\\etc");
	
	printf("mono_assembly_get_image\n");
	MonoImage *image = mono_assembly_get_image  (assembly);
	printf("Loading classes\n");
	botLoaderInfoClass = mono_class_from_name (image, "BlobbyBotloader", "BotLoadInfo");
	playerInputClass = mono_class_from_name (image, "BlobbyBotloader", "PlayerInput");
	worldStateInfoClass = mono_class_from_name (image, "BlobbyBotloader", "WorldState");
	exceptionClass = mono_class_from_name (image, "System", "Exception");
	
	exceptionMessage = mono_class_get_property_from_name(exceptionClass, "Message");
	exceptionStacktrace = mono_class_get_property_from_name(exceptionClass, "Stacktrace");
	
	printf("Loaded %i, %i and %i\n", (int)botLoaderInfoClass, (int)playerInputClass, (int)worldStateInfoClass);
	MonoMethodDesc *desc;
	
	printf("load LoadBot methods\n");
	MonoLoader_LoadHelper(loadMethod,loadMethodHandle,LoadBotMethod, "BlobbyBotloader.BotLoader:LoadBot(BlobbyBotloader.BotLoadInfo)")
	
	
	printf("load StepBot methods\n");
	// MonoLoader_LoadHelper(loadMethod,LoadBotMethod, "BlobbyBotloader.BotLoader:LoadBot(BlobbyBotloader.BotLoadInfo)")
	MonoLoader_LoadHelper(stepMethod,stepMethodHandle, StepBotMethod, "BlobbyBotloader.BotLoader:StepBot(BlobbyBotloader.BotManager,BlobbyBotloader.WorldState)")
	
	printf("load unloadBot methods\n");
	MonoLoader_LoadHelper(unloadMethod,unloadMethodHandle, UnloadBotMethod, "BlobbyBotloader.BotLoader:UnloadBot(BlobbyBotloader.BotManager)")
	
	printf("all loaded\n");
}

MonoLoader::~MonoLoader(){
	printf("cleanup mono...\n");
	mono_jit_cleanup (domain);
}

void MonoLoader::handleExn(const char* info, MonoException* exn, bool logSuccess){
	if (exn != NULL){
		MonoError monoerror;
		bool free_message = false;
		printf ("ERROR (%s): ", info);
		mono_print_unhandled_exception((MonoObject*)exn);
		printf ("print!");
		
		MonoException* exnI;
		MonoString* value = mono_object_to_string((MonoObject*)exn, (MonoObject**)&exnI);
		if (exnI != NULL){
			printf("What the fucking heck");
		}else {
			printf("Got string!");
		}
		
		char* error_msg = mono_string_to_utf8_checked (value, &monoerror);
		if (!mono_error_ok (&monoerror)) {
			mono_error_cleanup (&monoerror);
			error_msg = (char *) "some kind of mono error :/";
		} else {
			free_message = true;
		}
		printf("\n");
		printf(error_msg);
		if (free_message)
			mono_free(error_msg);
		printf("\n");
	} else {
		if (logSuccess){
			printf("%s succesfull\n", info);
		}
	}
}

void* MonoLoader::loadBot(BotLoadInfo& info) {
	MonoException* exn;
	//MonoObject* boxedInfo = boxLoadInfo(info);
	void *args [1];
	args[0] =  &info; // boxedInfo;//
	mono_thread_attach (domain);
	
	MonoObject* bot = 
		//loadMethod(boxedInfo, &exn);
		mono_runtime_invoke(loadMethodHandle, NULL, args, (MonoObject**)&exn);
	handleExn("loadbot", exn, true);
	void* botHandle= bot;
	printf("returning bot");
	return botHandle;	
}

PlayerInput MonoLoader::unboxPlayerInput(MonoObject* playerInputM){
	PlayerInput* handle = (PlayerInput*)mono_object_unbox(playerInputM);
	return *handle; // copy
}

MonoObject* MonoLoader::boxLoadInfo(BotLoadInfo& loadInfo){
	return mono_value_box(domain, botLoaderInfoClass, &loadInfo);
}

MonoObject* MonoLoader::boxWorldState(WorldState& worldState){
	return mono_value_box(domain, worldStateInfoClass, &worldState);
}

PlayerInput MonoLoader::getInput(void* bothandle, WorldState& status){
	MonoException* exn;
	//MonoObject* bot = (MonoObject*)bothandle;
	//MonoObject* boxedWorld = boxWorldState(status);
	void* args[2];
	args[0] = bothandle;
	args[1] = &status; // boxedWorld ;//
	mono_thread_attach (domain);
	
	MonoObject* boxedInput = 
		// stepMethod(bot, boxedWorld, &exn);
		mono_runtime_invoke(stepMethodHandle, NULL, args, (MonoObject**)&exn);
	PlayerInput input = unboxPlayerInput(boxedInput);
	handleExn("getInput", exn, false);
	return input;
}

void MonoLoader::unloadBot(void* bothandle) {
	MonoException* exn;
	//MonoObject* bot = (MonoObject*)bothandle;
	//unloadMethod(bot, &exn);
	void* args[1];
	args[0] = bothandle;
	mono_thread_attach (domain);
	
	mono_runtime_invoke(unloadMethodHandle, NULL, args, (MonoObject**)&exn);
	handleExn("unloadBot", exn, true);
}















