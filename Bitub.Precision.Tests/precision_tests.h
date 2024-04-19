#pragma once

#ifdef SINGLE_PRECISION
#define REALPRINT floatprint
#define REALRAND floatrand
#define NARROWRAND narrowfloatrand
#define UNIFORMRAND uniformfloatrand
#else
#define REALPRINT doubleprint
#define REALRAND doublerand
#define NARROWRAND narrowdoublerand
#define UNIFORMRAND uniformdoublerand
#endif // #ifdef SINGLE_PRECISION


double doublerand();