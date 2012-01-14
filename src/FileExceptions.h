/*=============================================================================
Blobby Volley 2
Copyright (C) 2006 Jonathan Sieber (jonathan_sieber@yahoo.de)

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

/*!	\class FileException
	\brief common base type for file exceptions
*/
class FileException: public std::exception {
	public:
		FileException(const std::string& f) : filename(f) {
		}
		
		virtual ~FileException() throw() {
		}
		const std::string& getFileName() const {
			return filename;
		} 
	private:
		std::string filename;
};

/*! \class FileLoadException
	\brief error thrown when a file could not be created
*/
class FileLoadException : public FileException
{
	public:
		FileLoadException(std::string name) : FileException(name)
		{
			error = "Couldn't load " + name;
		}
		
		virtual ~FileLoadException() throw() {}

		virtual const char* what() const throw()
		{
			return error.c_str();
		}
		
	private:
		std::string error;
};



/*! class PhysfsException
	\brief signales errors reported by physfs
	\details Exceptions of this type are thrown when calls of physfs functions
			report errors. what() gets the error string reportet by PHYSFS_getLastError()
*/
class PhysfsException : public FileException
{
	public:
		PhysfsException(const std::string& filename);
		
		~PhysfsException() throw() { };
		
		virtual const char* what() const throw()
		{
			return (getFileName() + ": " + physfsErrorMsg).c_str();
		}
	private:
	
		/// this string saves the error message
		std::string physfsErrorMsg;
};

/*!	\class NoFileOpenedException
	\brief signals operations on closed files
	\details Exceptions of this type are thrown when any file modifying or information querying
				functions are called without a file beeing opened. These are serious errors
				and generally, exceptions of this type should not occur, as it indicates logical 
				errors in the code. Still, this allows us to handle this situation without having
				to crash or exit.
	\sa File::check_file_open()
*/
class NoFileOpenedException : public FileException
{
	public:
		NoFileOpenedException() : FileException("") { };
		
		~NoFileOpenedException() throw() { };
		
		virtual const char* what() const throw()
		{
			// default error message for now
			return "trying to perform a file operation when no file was opened.";
		}
};

/*!	\class EOFException
	\brief thrown when trying to read over eof
	\details Exceptions of this type are issued when a reading operation tries to read
			behind a files eof.
*/
class EOFException : public FileException
{
	public:
		EOFException(const std::string& file) : FileException( file ) { };
		
		~EOFException() throw() { };
		
		virtual const char* what() const throw()
		{
			// default error message for now
			return (getFileName() + " trying to read after eof.").c_str();
		}
};
