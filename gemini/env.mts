const IS_AWS_LAMBDA = !!process.env.LAMBDA_TASK_ROOT;

export const CACHE_DIR = IS_AWS_LAMBDA ? '/tmp/cache' : 'cache';
export const RESULTS_DIR = IS_AWS_LAMBDA ? '/tmp/results' : 'results';
export const RESULT_FILE = IS_AWS_LAMBDA ? '/tmp/result.json' : 'result.json';
