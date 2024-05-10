#ifndef PRECISION_H
#define PRECISION_H

// *****************************************************************************
// Arbitrary Precision Floating-point Arithmetic               
// and Fast Robust Geometric Predicates by Jonathan Richard Shewchuk (1996)
//
// Adapted by Hung Si (2008) for use within the Tetgen implementation
// (Weierstrass Institute, Berlin)
// *****************************************************************************
// modified by Bernold Kraft (2014, 2024)
// *****************************************************************************


// The types 'intptr_t' and 'uintptr_t' are signed and unsigned integer types,
//   respectively. They are guaranteed to be the same width as a pointer.
//   They are defined in <stdint.h> by the C99 Standard.
//   However, Microsoft Visual C++ doesn't ship with this header file yet. We
//   need to define them. (Thanks to Steven G. Johnson from MIT for the 
//   following piece of code.) 

#ifdef _MSC_VER // Microsoft Visual C++
#ifdef _WIN64
typedef __int64 intptr_t;
typedef unsigned __int64 uintptr_t;
#else // not _WIN64
typedef int intptr_t;
typedef unsigned int uintptr_t;
#endif
#else // not Visual C++
#include <stdint.h>
#endif

#if LIB_EXPORT
#if _MSC_VER
#define LIB_EXPORTED __declspec(dllexport)
#else
#define LIB_EXPORTED __attribute__((__visibility__("default")))
#endif
#else
#if _MSC_VER
#define LIB_EXPORTED __declspec(dllimport)
#else
#define LIB_EXPORTED
#endif
#endif


#ifdef SINGLE_PRECISION
#define REAL float
#define AsREAL(value) (value##f)
#else
#define REAL double
#define AsREAL(value) (value)
#endif

// 3-dimensional vector
typedef struct {
	REAL x;
	REAL y;
	REAL z;
} Vec3;

// 2-dimensional vector
typedef struct {
	REAL x;
	REAL y;
} Vec2;

// EXPORT DECLARATION

#ifdef __cplusplus
extern "C" {
#endif

LIB_EXPORTED int exactsign(REAL value);

LIB_EXPORTED REAL exactinit();

LIB_EXPORTED REAL orient2dexact(Vec2 *pa, Vec2 *pb, Vec2 *pc);
LIB_EXPORTED REAL orient2dfast(Vec2 *pa, Vec2 *pb, Vec2 *pc);
LIB_EXPORTED REAL orient2d(Vec2 *pa, Vec2 *pb, Vec2 *pc);

LIB_EXPORTED REAL orient3dexact(Vec3 *pa, Vec3 *pb, Vec3 *pc, Vec3 *pd);
LIB_EXPORTED REAL orient3dfast(Vec3 *pa, Vec3 *pb, Vec3 *pc, Vec3 *pd);
LIB_EXPORTED REAL orient3d(Vec3 *pa, Vec3 *pb, Vec3 *pc, Vec3 *pd);

LIB_EXPORTED REAL insphereexact(Vec3 *pa, Vec3 *pb, Vec3 *pc, Vec3 *pd, Vec3 *pe);
LIB_EXPORTED REAL inspherefast(Vec3 *pa, Vec3 *pb, Vec3 *pc, Vec3 *pd, Vec3 *pe);
LIB_EXPORTED REAL insphere(Vec3 *pa, Vec3 *pb, Vec3 *pc, Vec3 *pd, Vec3 *pe);

LIB_EXPORTED REAL incircleexact(Vec2 *pa, Vec2 *pb, Vec2 *pc, Vec2 *pd);
LIB_EXPORTED REAL incirclefast(Vec2 *pa, Vec2 *pb, Vec2 *pc, Vec2 *pd);
LIB_EXPORTED REAL incircle(Vec2 *pa, Vec2 *pb, Vec2 *pc, Vec2 *pd);

#ifdef __cplusplus
}
#endif

#endif