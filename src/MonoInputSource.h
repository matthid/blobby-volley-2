/*=============================================================================
Mono Bots for Blobby Volley 2 
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

#include "Global.h"
#include "InputSource.h"
#include "Vector.h"
#include "MonoLoader.h"

#include <boost/circular_buffer.hpp>

#include <string>

/// \class ScriptedInputSource
/// \brief Bot controller
/// \details ScriptedInputSource provides an implementation of InputSource, which uses
/// Lua scripts to get its input. The given script is automatically initialised
/// and provided with an interface to the game.

/// The API documentation can now be found in doc/ScriptAPI.txt

class DuelMatch;

class MonoInputSource : public InputSource
{
public:
	/// The constructor automatically loads and initializes the script
	/// with the given filename. The side parameter tells the script
	/// which side is it on.
	MonoInputSource(const std::string& filename, PlayerSide side, unsigned int difficulty);
	~MonoInputSource();
	
	virtual PlayerInput getNextInput();
	
private:

	MonoVector2 convertVector2(Vector2 vector);
	void* botHandle;
	MonoLoader* loader;
};
