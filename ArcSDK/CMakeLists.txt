cmake_minimum_required(VERSION 3.15)
project(ArcSDK)

find_program(GITVERSION NAMES gitversion dotnet-gitversion)

if(NOT GITVERSION STREQUAL "GITVERSION-NOTFOUND")
    message(${GITVERSION})

    exec_program(${GITVERSION} ${CMAKE_CURRENT_SOURCE_DIR} ARGS /showvariable Major OUTPUT_VARIABLE VERSION_MAJOR)
    exec_program(${GITVERSION} ${CMAKE_CURRENT_SOURCE_DIR} ARGS /showvariable Minor OUTPUT_VARIABLE VERSION_MINOR)
    exec_program(${GITVERSION} ${CMAKE_CURRENT_SOURCE_DIR} ARGS /showvariable Patch OUTPUT_VARIABLE VERSION_PATCH)
    exec_program(${GITVERSION} ${CMAKE_CURRENT_SOURCE_DIR} ARGS /showvariable PreReleaseLabel OUTPUT_VARIABLE VERSION_PRERELEASE)
    exec_program(${GITVERSION} ${CMAKE_CURRENT_SOURCE_DIR} ARGS /showvariable PreReleaseNumber OUTPUT_VARIABLE VERSION_PRERELEASE_NUMBER)
    exec_program(${GITVERSION} ${CMAKE_CURRENT_SOURCE_DIR} ARGS /showvariable BuildMetaData OUTPUT_VARIABLE VERSION_BUILDMETA)

    string(APPEND VERSION_PRERELEASE ${VERSION_PRERELEASE_NUMBER})
    string(STRIP "${VERSION_PRERELEASE}" VERSION_PRERELEASE)
    string(STRIP "${VERSION_BUILDMETA}" VERSION_BUILDMETA)
else()
    set(VERSION_MAJOR 0 CACHE STRING "The major version number")
    set(VERSION_MINOR 0 CACHE STRING "The minor version number")
    set(VERSION_PATCH 0 CACHE STRING "The patch version number")
    set(VERSION_PRERELEASE "" CACHE STRING "The prerelease version")
    set(VERSION_BUILDMETA "" CACHE STRING "The buildmeta version")
endif()

set(VERSION "${VERSION_MAJOR}.${VERSION_MINOR}.${VERSION_PATCH}")

if(VERSION_PRERELEASE)
    string(APPEND VERSION "-${VERSION_PRERELEASE}")
endif()

if(VERSION_BUILDMETA)
    string(APPEND VERSION "+${VERSION_BUILDMETA}")
endif()

string(TIMESTAMP CURRENT_YEAR "%Y")

configure_file(${CMAKE_CURRENT_SOURCE_DIR}/version.rc.in ${CMAKE_CURRENT_BINARY_DIR}/version.rc)

set(SUBHOOK_STATIC ON)
set(SUBHOOK_TESTS OFF)
add_subdirectory(subhook)
set_property(TARGET subhook PROPERTY MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")

file(GLOB_RECURSE HEADERS CONFIGURE_DEPENDS src/*.h src/*.hpp)
file(GLOB_RECURSE SOURCES CONFIGURE_DEPENDS src/*.c src/*.cpp)
set(FILES ${HEADERS} ${SOURCES} ${CMAKE_CURRENT_BINARY_DIR}/version.rc)

add_library(${PROJECT_NAME} SHARED ${FILES})
target_link_libraries(${PROJECT_NAME} subhook)
set_property(TARGET ${PROJECT_NAME} PROPERTY CXX_STANDARD 17)
set_property(TARGET ${PROJECT_NAME} PROPERTY MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")