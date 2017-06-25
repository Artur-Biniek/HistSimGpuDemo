kernel void annualizedReturns 
(
	global			float * output,
    global	const	float * input,
	global	const	float * mv,
			const	float   ann,	                                
            const	int     holding)
{   
    unsigned int gx = get_global_id(0);
	unsigned int gy = get_global_id(1);

	unsigned int sx = get_global_size(0);
	
	unsigned int lx = get_local_id(0);

    unsigned int insizex = sx + holding;

	unsigned int base_addr = gx + insizex * gy;

    float pt = input[base_addr];
    float pt_h = input[base_addr + holding]; 

	float ri_ann = pow(pt_h / pt, ann);
         
    output[gx + sx * gy] = (ri_ann + 1) * mv[gy];
}

kernel void aggregateReturnsKernel
(
	global	float * data,
	const	int		sizeY)
{   
    unsigned int gx = get_global_id(0);
	
	unsigned int sx = get_global_size(0);	

	float sum  = 0; 
	
	for (int i = 0; i < sizeY; i++)
	{
		sum += data[gx + sx * i];
	}
	
	data[gx] = sum;
}