#include <random>
#include <climits>

#include <precision.h>

#include "precision_tests.h"

static std::default_random_engine generator;
static std::uniform_int_distribution<long> distribution(LONG_MIN, LONG_MAX);

/*****************************************************************************/
/*                                                                           */
/*  doublerand()   Generate a double with random 53-bit significand and a    */
/*                 random exponent in [0, 511].                              */
/*                                                                           */
/*****************************************************************************/

double doublerand()
{
    double result;
    double expo;
    long a, b, c;
    long i;

    a = distribution(generator);
    b = distribution(generator);
    c = distribution(generator);

    result = (double)(a - 1073741824) * 8388608.0 + (double)(b >> 8);
    for (i = 512, expo = 2; i <= 131072; i *= 2, expo = expo * expo) {
        if (c & i) {
            result *= expo;
        }
    }
    return result;
}

/*****************************************************************************/
/*                                                                           */
/*  doubleprint()   Print the bit representation of a double.                */
/*                                                                           */
/*  Useful for debugging exact arithmetic routines.                          */
/*                                                                           */
/*****************************************************************************/

void doubleprint(double number)
{
    unsigned long long no;
    unsigned long long sign, expo;
    int exponent;
    int i, bottomi;

    no = *(unsigned long long*) & number;
    sign = no & 0x8000000000000000ll;
    expo = (no >> 52) & 0x7ffll;
    exponent = (int)expo;
    exponent = exponent - 1023;
    if (sign) {
        printf("-");
    }
    else {
        printf(" ");
    }
    if (exponent == -1023) {
        printf(
            "0.0000000000000000000000000000000000000000000000000000_     (   )");
    }
    else {
        printf("1.");
        bottomi = -1;
        for (i = 0; i < 52; i++) {
            if (no & 0x0008000000000000ll) {
                printf("1");
                bottomi = i;
            }
            else {
                printf("0");
            }
            no <<= 1;
        }
        printf("_%d  (%d)", exponent, exponent - 1 - bottomi);
    }
}


/*****************************************************************************/
/*                                                                           */
/*  floatprint()   Print the bit representation of a float.                  */
/*                                                                           */
/*  Useful for debugging exact arithmetic routines.                          */
/*                                                                           */
/*****************************************************************************/


void floatprint(float number)
{
    unsigned no;
    unsigned sign, expo;
    int exponent;
    int i, bottomi;

    no = *(unsigned*)&number;
    sign = no & 0x80000000;
    expo = (no >> 23) & 0xff;
    exponent = (int)expo;
    exponent = exponent - 127;
    if (sign) {
        printf("-");
    }
    else {
        printf(" ");
    }
    if (exponent == -127) {
        printf("0.00000000000000000000000_     (   )");
    }
    else {
        printf("1.");
        bottomi = -1;
        for (i = 0; i < 23; i++) {
            if (no & 0x00400000) {
                printf("1");
                bottomi = i;
            }
            else {
                printf("0");
            }
            no <<= 1;
        }
        printf("_%3d  (%3d)", exponent, exponent - 1 - bottomi);
    }
}


/*****************************************************************************/
/*                                                                           */
/*  expansion_print()   Print the bit representation of an expansion.        */
/*                                                                           */
/*  Useful for debugging exact arithmetic routines.                          */
/*                                                                           */
/*****************************************************************************/


void expansion_print(int elen, REAL* e)
{
    int i;

    for (i = elen - 1; i >= 0; i--) {
        REALPRINT(e[i]);
        if (i > 0) {
            printf(" +\n");
        }
        else {
            printf("\n");
        }
    }
}

/*****************************************************************************/
/*                                                                           */
/*  narrowdoublerand()   Generate a double with random 53-bit significand    */
/*                       and a random exponent in [0, 7].                    */
/*                                                                           */
/*****************************************************************************/


double narrowdoublerand()
{
    double result;
    double expo;
    long a, b, c;
    long i;

    a = distribution(generator);
    b = distribution(generator);
    c = distribution(generator);
    result = (double)(a - 1073741824) * 8388608.0 + (double)(b >> 8);
    for (i = 512, expo = 2; i <= 2048; i *= 2, expo = expo * expo) {
        if (c & i) {
            result *= expo;
        }
    }
    return result;
}


/*****************************************************************************/
/*                                                                           */
/*  uniformdoublerand()   Generate a double with random 53-bit significand.  */
/*                                                                           */
/*****************************************************************************/


double uniformdoublerand()
{
  double result;
  long a, b;

  a = distribution(generator);
  b = distribution(generator);
  result = (double) (a - 1073741824) * 8388608.0 + (double) (b >> 8);
  return result;
}


/*****************************************************************************/
/*                                                                           */
/*  floatrand()   Generate a float with random 24-bit significand and a      */
/*                random exponent in [0, 63].                                */
/*                                                                           */
/*****************************************************************************/


float floatrand()
{
  float result;
  float expo;
  long a, c;
  long i;

  a = distribution(generator);
  c = distribution(generator);
  result = (float) ((a - 1073741824) >> 6);
  for (i = 512, expo = 2; i <= 16384; i *= 2, expo = expo * expo) {
    if (c & i) {
      result *= expo;
    }
  }
  return result;
}


/*****************************************************************************/
/*                                                                           */
/*  narrowfloatrand()   Generate a float with random 24-bit significand and  */
/*                      a random exponent in [0, 7].                         */
/*                                                                           */
/*****************************************************************************/


float narrowfloatrand()
{
  float result;
  float expo;
  long a, c;
  long i;

  a = distribution(generator);
  c = distribution(generator);
  result = (float) ((a - 1073741824) >> 6);
  for (i = 512, expo = 2; i <= 2048; i *= 2, expo = expo * expo) {
    if (c & i) {
      result *= expo;
    }
  }
  return result;
}


/*****************************************************************************/
/*                                                                           */
/*  uniformfloatrand()   Generate a float with random 24-bit significand.    */
/*                                                                           */
/*****************************************************************************/


float uniformfloatrand()
{
  float result;
  long a;

  a = distribution(generator);
  result = (float) ((a - 1073741824) >> 6);
  return result;
}
