#include <stdio.h>
#include <stdlib.h>

#include "precision_ext.h"

/*****************************************************************************/
/*                                                                           */
/* Additional predicates using Adaptive Precision Logic                      */
/* Bernold Kraft (2020)                                                      */
/*                                                                           */
/*****************************************************************************/

/*
 * Scalar projection of pc onto ray of pa -> pb
 * Returns: scalar projection times length of ray
 */
REAL project_3d(Vec3* pa, Vec3* pb, Vec3* pc)
{
	INEXACT REAL bvirt;
	REAL avirt, bround, around;
	INEXACT REAL c;
	INEXACT REAL abig;
	REAL a0hi, a0lo, a1hi, a1lo, bhi, blo;
	REAL err1, err2, err3;
	INEXACT REAL _i, _j, _k, _l, _m, _n;
	REAL _0, _1, _2;

	REAL abx, aby, abz, acx, acy, acz;
	REAL abxtail, abytail, abztail, acxtail, acytail, acztail;

	// Two-Diff pb, pa => ab
	Two_Diff(pb->x, pa->x, abx, abxtail);
	Two_Diff(pb->y, pa->y, aby, abytail);
	Two_Diff(pb->z, pa->z, abz, abztail);
	// Two-Diff pc, pa => ac
	Two_Diff(pc->x, pa->x, acx, acxtail);
	Two_Diff(pc->y, pa->y, acy, acytail);
	Two_Diff(pc->z, pa->z, acz, acztail);

	REAL abacx[8], abacy[8], abacz[8];
	// Two-Two-Product ab.ac
	Two_Two_Product(abx, abxtail, acx, acxtail,
		abacx[7], abacx[6], abacx[5], abacx[4],
		abacx[3], abacx[2], abacx[1], abacx[0]);

	Two_Two_Product(aby, abytail, acy, acytail,
		abacy[7], abacy[6], abacy[5], abacy[4],
		abacy[3], abacy[2], abacy[1], abacy[0]);

	Two_Two_Product(abz, abztail, acz, acztail,
		abacz[7], abacz[6], abacz[5], abacz[4],
		abacz[3], abacz[2], abacz[1], abacz[0]);

	// Expansion-Sum components
	REAL temp16[16];
	int templen16 = fast_expansion_sum_zeroelim(8, abacx, 8, abacy, temp16);
	REAL temp24[24];
	int templen24 = fast_expansion_sum_zeroelim(templen16, temp16, 8, abacz, temp24);

	return temp24[templen24 - 1];
}

/*
 * Shortest distance of pc to pa -> pb
 * Returns: scalar tangent length times length of ray
 */
REAL tangent_3d(Vec3* pa, Vec3* pb, Vec3* pc)
{
	// Two-Diff pb, pa => ab
	// Two-Diff pc, pa => ac

	return 0;
}