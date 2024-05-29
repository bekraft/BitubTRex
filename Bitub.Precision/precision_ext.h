#ifndef PRECISION_EXT_H
#define PRECISION_EXT_H

#include "precision_internal.h"

#ifdef DLL_EXPORT
#ifdef __cplusplus
extern "C" {
#endif
#endif

// Additional predicates using Adaptive Precision Logic
LIB_EXPORTED REAL project_3d(Vec3* pa, Vec3* pb, Vec3* pc);
LIB_EXPORTED REAL tangent_3d(Vec3* pa, Vec3* pb, Vec3* pc);

#ifdef DLL_EXPORT
#ifdef __cplusplus
}
#endif
#endif

#endif