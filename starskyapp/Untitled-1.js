
// export default class FetchExample extends React.Component {

//   constructor(props){
//     super(props);
//     this.state ={ isLoading: true}
//   }
 
//   static navigationOptions = ({ navigation, navigationOptions }) => {
//     const { params } = navigation.state;

//     return {
//       title: params ? params.otherParam : 'A Nested Details Screen',
//       /* These values are used instead of the shared configuration! */
//       headerStyle: {
//         backgroundColor: '#f4511e',
//       },
//       headerTintColor: '#fff',
//     };
//   };

//   componentWillMount() {
//     // navigationOptions.setParams({
//     //   title: `Badge Count`,
//     // });
//   }



//   componentDidMount(){

//     return fetch('http://localhost:5000/api/folder?f=/2018')
//       .then((response) => response.json())
//       .then((responseJson) => {

//         this.setState({
//           isLoading: false,
//           dataSource: responseJson
//         }, function(){

//         });

//         const { params } = navigation.state;


//         params.setState({
//           title: "sf"
//         }, function(){

//         });
//         // this.props.navigation.navigate('HomeScreen', {title: 'WHATEVER'})

//         // navigationOptions.title = "ss"

//       })
//       .catch((error) =>{
//         console.error(error);
//       });
//   }

//   render(){

//     if(this.state.isLoading){
//       return(
//         <View style={{flex: 1, padding: 20}}>
//           <ActivityIndicator/>
//         </View>
//       )
//     }

//     return(
//       <View style={{flex: 1, paddingTop:20}}>

//         <Button
//           title="Go to Details... again"
//           onPress={() => this.props.navigation.navigate('Details')}
//         />

//           <Button
//         title="Update the title"
//         onPress={() => this.setState({isLoading: true})}
//           />

//         <FlatList
//           data={this.state.dataSource}
//           renderItem={({item}) => <Text>{item.fileName}, {item.filePath}</Text>}
//           keyExtractor={(item, index) => index}
//         />
//       </View>
//     );
//   }
// }