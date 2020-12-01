export interface Inital {
	library: any;
	apiCallsInProgress: number;
}

const initalState = {
	library: {},
	apiCallsInProgress: 0,
} as Inital;

export default initalState;
