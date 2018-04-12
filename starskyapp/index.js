import React, { Component,PropTypes } from 'react';

import { AppRegistry} from 'react-native';

import { StackNavigator, DrawerNavigator } from 'react-navigation';
import HomeScreen from './HomeScreen';
import DetailScreen from './DetailScreen';
 
// import tabNav from './tabnav';
import stacknav from './stacknav';

const drawernav = DrawerNavigator({
  DrawerItem1: {
      screen: stacknav,
      navigationOptions: {
          drawerLabel: "Drawer Item 1",
      },
  }
});


// const RootStack = StackNavigator(
//   {
//     Home: {
//       screen: HomeScreen,
//     },
//     Details: {
//       screen: DetailScreen,
//     },
//   },
//   {
//     initialRouteName: 'Home',
//     navigationOptions: {
//       headerStyle: {
//         backgroundColor: '#f4511e',
//       },
//       headerTintColor: '#fff',
//       headerTitleStyle: {
//         fontWeight: 'bold',
//       },
//     },
//   }
// );

// class App extends React.Component {
//     render() {
//         return <RootStack />;
//     }
// }


AppRegistry.registerComponent('starskyapp', () => drawernav);
