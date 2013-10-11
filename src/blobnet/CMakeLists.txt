set (blobnet_SRC
	layer/Http.cpp layer/Http.hpp
	)
include_directories(
	${Boost_INCLUDE_DIR} )
add_library(blobnet STATIC ${blobnet_SRC})	
