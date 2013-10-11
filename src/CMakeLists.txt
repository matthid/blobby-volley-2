add_subdirectory(lua)
add_subdirectory(tinyxml)
add_subdirectory(raknet)
add_subdirectory(blobnet)

add_definitions(-DTIXML_USE_STL)
include_directories(.)

SET(CMAKE_C_FLAGS "-g ${CMAKE_C_FLAGS}")
SET(CMAKE_CXX_FLAGS "-g ${CMAKE_CXX_FLAGS}")

set(common_SRC	
	Global.h
	BlobbyDebug.cpp BlobbyDebug.h
	Clock.cpp Clock.h
	DuelMatch.cpp DuelMatch.h
	ReplayRecorder.cpp ReplayRecorder.h
	NetworkGame.cpp NetworkGame.h
	NetworkMessage.cpp NetworkMessage.h
	GameLogic.cpp GameLogic.h
	PhysicWorld.cpp PhysicWorld.h
	SpeedController.cpp SpeedController.h
	UserConfig.cpp UserConfig.h
	File.cpp File.h
	FileWrite.cpp FileWrite.h
	FileRead.cpp FileRead.h
	FileSystem.cpp FileSystem.h
	PhysicState.cpp PhysicState.h
	DuelMatchState.cpp DuelMatchState.h
	GameLogicState.cpp GameLogicState.h
	GenericIO.cpp GenericIO.h
	ReplaySavePoint.cpp ReplaySavePoint.h
	InputSource.cpp InputSource.h
	PlayerIdentity.cpp PlayerIdentity.h
	)
	
set (blobby_SRC ${common_SRC}
	Blood.cpp Blood.h
	TextManager.cpp TextManager.h
	main.cpp
	IMGUI.cpp IMGUI.h
	LagDetectionSystem.cpp LagDetectionSystem.h
	InputDevice.cpp InputDevice.h
	InputManager.cpp InputManager.h
	LocalInputSource.cpp LocalInputSource.h
	RenderManager.cpp RenderManager.h
	RenderManagerGL2D.cpp RenderManagerGL2D.h
	RenderManagerGP2X.cpp RenderManagerGP2X.h
	RenderManagerSDL.cpp RenderManagerSDL.h
	MonoLoader.cpp MonoLoader.h
	MonoInputSource.cpp MonoInputSource.h
	ScriptedInputSource.cpp ScriptedInputSource.h	
	BotAPICalculations.cpp BotAPICalculations.h
	SoundManager.cpp SoundManager.h
	Vector.h
	ReplayRecorder.cpp ReplayRecorder.h
	ReplayPlayer.cpp ReplayPlayer.h
	ReplayLoader.cpp
	InputSourceFactory.cpp InputSourceFactory.h
	state/State.cpp state/State.h
	state/LocalGameState.cpp state/LocalGameState.h
	state/NetworkState.cpp state/NetworkState.h
	state/OptionsState.cpp state/OptionsState.h
	state/NetworkSearchState.cpp state/NetworkSearchState.h
	state/ReplayState.cpp state/ReplayState.h
	state/ReplaySelectionState.cpp state/ReplaySelectionState.h
	)
	
set (blobby-server_SRC ${common_SRC}
	server/DedicatedServer.cpp server/DedicatedServer.h
	server/NetworkPlayer.cpp server/NetworkPlayer.h
	)

find_package(Boost REQUIRED)
find_package(PhysFS REQUIRED)
find_package(SDL REQUIRED)
find_library(MONO_INCLUDE_DIR mono/jit/jit.h)
find_package(OpenGL)

find_library(MONO_LIBRARY libmono-2.0.a)
if (OPENGL_FOUND)
	add_definitions(-DHAVE_LIBGL)
else ()
	
endif (OPENGL_FOUND)
	set(OPENGL_INCLUDE_DIR ".")
include_directories(
	${Boost_INCLUDE_DIR} ${PHYSFS_INCLUDE_DIR}
	${SDL_INCLUDE_DIR} ${OPENGL_INCLUDE_DIR}
	${MONO_INCLUDE_DIR}
)

if (OPENGL_FOUND)
	add_definitions(-DHAVE_LIBGL)
endif (OPENGL_FOUND)

if (CMAKE_SYSTEM_NAME STREQUAL Windows)
	set(RAKNET_LIBRARIES raknet ws2_32)
else (CMAKE_SYSTEM_NAME STREQUAL Windows)
	set(RAKNET_LIBRARIES raknet)
endif (CMAKE_SYSTEM_NAME STREQUAL Windows)

# set(LINK_FLAGS "-mno-cygwin -mms-bitfields -mwindows -L/usr/local/lib -lmono-2.0 -lws2_32 -lpsapi -lole32 -lwinmm -loleaut32 -ladvapi32 -lversion")
# -mno-cygwin -mwindows -L/usr/local/lib
# set(CMAKE_EXE_LINKER_FLAGS "-v -L/cygdrive/c/cygwin/usr/local/lib -mwindows -lSDLmain -lSDL -lm -luser32 -lgdi32 -lwinmm -mms-bitfields -lmono-2.0 -lws2_32 -lpsapi -lole32 -loleaut32 -ladvapi32 -lversion")
set(CMAKE_EXE_LINKER_FLAGS "-g -m32 -mms-bitfields -mwindows -static \"-L/cygdrive/c/Program Files (x86)/Mono-3.2.3/lib\" \"-L/cygdrive/c/Program Files (x86)/Mono-3.2.3/bin\" -L/cygdrive/c/projects/blobby2mono/libs/sdlold/build/.libs -L/cygdrive/c/projects/blobby2mono/libs/libzip/lib/.libs -L/cygdrive/c/projects/blobby2mono -L/cygdrive/c/projects/blobby2mono/libs/physfs")
set(LINK_SEARCH_END_STATIC)

set(CMAKE_EXE_LINK_DYNAMIC_C_FLAGS)       # remove -Wl,-Bdynamic
set(CMAKE_EXE_LINK_DYNAMIC_CXX_FLAGS)
set(CMAKE_SHARED_LIBRARY_C_FLAGS)         # remove -fPIC
set(CMAKE_SHARED_LIBRARY_CXX_FLAGS)
set(CMAKE_SHARED_LIBRARY_LINK_C_FLAGS)    # remove -rdynamic
set(CMAKE_SHARED_LIBRARY_LINK_CXX_FLAGS)

add_executable(blobby ${blobby_SRC})
target_link_libraries(blobby lua raknet blobnet tinyxml ${RAKNET_LIBRARIES}  ${PHYSFS_LIBRARY} ${SDL_LIBRARY} ${OPENGL_LIBRARIES} ${MONO_LIBRARY} zip z stdc++ wsock32 ws2_32 psapi ole32 winmm oleaut32 advapi32 version)

if (UNIX)
	add_executable(blobby-server ${blobby-server_SRC})
	target_link_libraries(blobby-server lua raknet tinyxml ${RAKNET_LIBRARIES} ${PHYSFS_LIBRARY} ${SDL_LIBRARY} stdc++ ws2_32 zip z)
endif (UNIX)

if (CMAKE_SYSTEM_NAME STREQUAL Windows)
	set_target_properties(blobby PROPERTIES LINK_FLAGS "-mwindows") # disable the console window
endif (CMAKE_SYSTEM_NAME STREQUAL Windows)

if (WIN32)
	install(TARGETS blobby DESTINATION .)
elseif (UNIX)
	install(TARGETS blobby blobby-server DESTINATION bin)
endif (WIN32)
