import React, { Component,PropTypes } from 'react';

import { AppRegistry} from 'react-native';

import { StackNavigator } from 'react-navigation';
import HomeScreen from './HomeScreen';
import DetailScreen from './DetailScreen';


// // Screen connecting with rendering component.
// class UsersListScreen extends Component {
//     onUserPress = (user) => {
//       this.props.navigation.navigate('UserProfile', {user: user});
//     }
//     render() {
//       return <UserList onUserPress={this.onUserPress} />
//     }
//   }
  
//   // Rendering component.
//   function UserList(props) {
//     const users = getUsersFromSomewhere();
//     return (
//       <View>
//         {users.map(user => {
//           return (
//             <Button title={user.name} onPress={() => props.onUserPress(user)} />
//           )
//         })}
//       </View>
//     );
//   }
//   UserList.propTypes = {
//     onUserPress: PropTypes.func.isRequired
//   }
  
//   // Screen connecting with rendering component.
//   class UserProfileScreen extends Component {
//     onSendMessagePress = (user) => {
//       this.props.navigation.navigate('SendMessage', {user: user})
//     }
//     render() {
//       return <UserProfile 
//                 user={this.props.navigation.state.params.user}
//                 onSendMessagePress={this.onSendMessagePress}
//               />
//     }
//   }
  
//   // Rendering component.
//   function UserProfile(props) {
//     return (
//       <View>
//         <Text>props.user.name</Text>
//         <Button title="Send message" onPress={() => props.onSendMessagePress(props.user)} />
//       </View>
//     );
//   }
//   UserProfile.propTypes = {
//     user: PropTypes.object.isrequired,
//     onSendMessagePress: PropTypes.func.isrequired
//   }
  
//   // Navigation stack.
//   const MyStack = StackNavigator({
//     UsersList: {
//       screen: UsersListScreen
//     },
//     UserProfile: {
//       screen: UserProfileScreen
//     },
//     SendMessage: {
//       screen: SendMessageScreen
//     }
//   });
  

  

const RootStack = StackNavigator(
  {
    Home: {
      screen: HomeScreen,
    },
    Details: {
      screen: DetailScreen,
    },
  },
  {
    initialRouteName: 'Home',
    navigationOptions: {
      headerStyle: {
        backgroundColor: '#f4511e',
      },
      headerTintColor: '#fff',
      headerTitleStyle: {
        fontWeight: 'bold',
      },
    },
  }
);

class App extends React.Component {
    render() {
        return <RootStack />;
    }
}


AppRegistry.registerComponent('starskyapp', () => App);
